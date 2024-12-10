// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit, HostListener} from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zwischen den Routen zu navigieren
import { ApiConfigService } from '@service/API-URL.service'; // ApiConfigService verwaltet global die API-URLs für die Kommunikation mit der Backend-API
import { WebSocketService } from '@service/websocket.service'; // Import des WebSocketService zur Echtzeit-Kommunikation mit dem Server
import { lastValueFrom } from 'rxjs';

// Deklaration der Komponente mit Metadaten
@Component({
  selector: 'app-display-dashboard', // Der Selector ist der Name der Komponente, der im HTML verwendet wird
  templateUrl: './display-dashboard.component.html', // Das Template (HTML) für diese Komponente
  styleUrls: ['./display-dashboard.component.css'] // Das CSS für das Styling dieser Komponente
})
export class DisplayDashboardComponent implements OnInit {

  isLoading: boolean = true; // Indikator, ob Daten noch geladen werden
  AmplenExsit: boolean = false; // Indikator, ob es Ampeln auf dem aktuellen Board gibt

  selectedID: number = 0; // Hält die ID des ausgewählten Dashboards
  selectedNAME: string = ""; // Hält den Namen des ausgewählten Dashboards
  selectedIMG: string = ""; // Hält den Bildpfad des ausgewählten Dashboards
  aspectRatio: number = 0; // Hält das Seitenverhältnis des ausgewählten Dashboards
  screenAspectRatio: number = 0; // Hält das Seitenverhältnis des Browserfensters

  // Arrays für die abgerufenen Dashboards und Ampeln
  Dashboards: any[] = []; // Speichert die abgerufenen Dashboards
  Ampeln: any[] = []; // Speichert die abgerufenen Ampeln

  // Der Konstruktor, der die Abhängigkeiten für diese Komponente injiziert
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService, private webSocketService: WebSocketService) { }

  ngOnInit() {
    // Lädt die Dashboards beim Start der Komponente
    this.refreshBoards();      
  }



  // Methode zum Abrufen der Dashboards-Daten von der API
  refreshBoards() {
    // HTTP GET-Anfrage an die API, um alle Dashboards abzurufen
    this.http.get<any[]>(this.apiConfig.DB_APIUrl + 'getDashboards')
      .subscribe(data => { // Wenn die Antwort erfolgreich ist
        this.Dashboards = data; // Speichern der empfangenen Daten in der Dashboards-Variable
        // console.log("API Response:", data); // Zur Kontrolle der API-Antwort in der Konsole
      });
  }







  // Methode, die aufgerufen wird, wenn der Benutzer ein Dashboard auswählt
  async viewBoard(ID: number, NAME: string, IMG_PATH: string, ratio: string) {

    this.isLoading = true; //Lade Spinner aktivieren

    // Setze die Variablen für die ausgewählten Dashboard-Daten
    this.selectedID = ID;
    this.selectedNAME = NAME;
    this.selectedIMG = IMG_PATH;
    this.aspectRatio = Number(ratio);

    console.log("Ausgewählte Dashboard-ID:" + this.selectedID);


    // Aktualisiere die Dashboards und lade die Ampeln für das ausgewählte Dashboard
    await this.refreshBoards();
    await this.getAmpelnVonBoard();

    if (this.AmplenExsit) { //WebSocket nur aufbauen, wenn Ampeln existieren

        // WebSocket-Verbindung zum OPC-Controller herstellen
        //this.webSocketService.connect(this.apiConfig.OPC_APIUrl + 'connectWebSocket', this.OPC_AddArray); // Server-Adresse
        this.webSocketService.connect(this.apiConfig.OPC_APIUrl + 'connectWebSocket', this.OPC_AddArray); // Server-Adresse + OPC_AddArray

        // Die empfangene Nachricht (event.data) wird als JSON interpretiert und in ein 2D-Array number[][] umgewandelt
        this.webSocketService.socket.onmessage = (event) => {
          this.OPC_BITArray = JSON.parse(event.data);
          console.log('Neuste OPC Daten:', this.OPC_BITArray)
      };


        //Seitenverhältnis der Anzeige auf Bildschirm anpassen
        setTimeout(() => {
          this.fitScreenAspectRatio();
        }, 0); // Verzögerung von 0 ms sorgt dafür, dass der DOM-Rendering abgeschlossen ist

    }


    this.isLoading = false; //Lade Spinner deaktivieren
  }





  // Methode, um die Auswahl zurückzusetzen und auf den ursprünglichen Zustand zurückzusetzen
  async clearSelection() {
    // Setze alle Variablen auf ihre Standardwerte zurück
    this.selectedID = 0;
    this.selectedNAME = "";
    this.selectedIMG = "";
    this.aspectRatio = 0;
    this.AmplenExsit = false;

    this.Ampeln = [];

    this.colorsArray = [];
    this.OPC_AddArray = [];
    this.OPC_BITArray = [];

    //Schließe Websocket Verbindung
    this.webSocketService.closeConnection();
  }






  //Methode um die darstellung des Layoutbilds an das Boweserfenster anzupassen
  fitScreenAspectRatio() {
    this.screenAspectRatio = window.innerWidth / window.innerHeight; //Berechne das Seitenverhältnis vom Browserfenster

    const dashboardContainer = document.querySelector('.dashboard-container') as HTMLElement;

    if (this.aspectRatio > this.screenAspectRatio) {
      // Layoutbild ist breiter als der Bildschirm, setze die Breite auf 100%
      dashboardContainer.style.setProperty('--width', '100%');
      dashboardContainer.style.setProperty('--height', 'auto');
    } else {
      // Layoutbild ist schmaler als der Bildschirm, setze die Höhe auf 90vh (wegen oberem Menü)
      dashboardContainer.style.setProperty('--height', '90vh');
      dashboardContainer.style.setProperty('--width', 'auto');
    }
  }

  @HostListener('window:resize', ['$event'])
  onResize(event: Event) {
    if (this.AmplenExsit) {
      this.fitScreenAspectRatio();
    }
  }






  // Arrays zur Speicherung von Farb- und OPC-Daten für Ampeln
  colorsArray: string[][] = []; // Ein Array von Arrays, das Farben für jede Ampel speichert
  OPC_AddArray: string[][] = []; // Ein Array von Arrays, das die OPC-Addressen speichert
  OPC_BITArray: number[][] = []; // Ein Array von Arrays, das die OPC-Status-Bits der Addressen speichert

  // Methode zum Abrufen der Ampel-Daten von der API
  async getAmpelnVonBoard() {

    try {
      // Warten auf die Antwort der HTTP-Anfrage
      const data = await lastValueFrom(
        this.http.get<any[]>(`${this.apiConfig.DB_APIUrl}getAmpeln?selected_ID=${this.selectedID}`)
      );

      // Speichern der Ampel-Daten in der Ampeln-Variable
      this.Ampeln = data;
      console.log("API Response:", data); // Ausgabe der Antwort zur Kontrolle

      // Wenn Ampeln vorhanden sind, konvertiere die COLOR- und OPC_BIT-Strings in Arrays
      if (this.Ampeln.length > 0) {
        this.AmplenExsit = true;

        // Iteriere über alle Ampeln und konvertiere die Farb- und OPC-Bit-Strings
        for (let ampeln of this.Ampeln) {
          //  Wandle den COLORS-String in Array
          this.colorsArray.push(ampeln.COLORS.split(','));

          // Wandle den OPC_BIT-String in Array
          this.OPC_AddArray.push(ampeln.OPC_TagList.split(',').map((item: string) => ampeln.OPC_Addr + '.' + item));
        }
      }
    } catch (error) {
      // Fehlerbehandlung: Zeige eine Fehlermeldung, falls die Anfrage fehlschlägt
      console.error("Fehler beim Abrufen der Ampeln:", error);
      alert("Fehler beim Abrufen der Ampeln. Bitte versuche es erneut.");
    }

    //console.log("colorsArray:", this.colorsArray);
    console.log("OPC_AddArray:", this.OPC_AddArray);

    //OPC_BITArray initalisieren: die Struktur von OPC_AddArray kopieren und alle Strings durch 1 ersetzen
    this.OPC_BITArray = this.OPC_AddArray.map(row => row.map(() => 1));

  }







  // Methode, um die Styles für jedes Ampel-Element zu berechnen
  getElementStyles(element: any) {
   const height = element.ColorCount * element.SIZE * 1.32  +  element.SIZE*0.8; // Höhe wird von der Breite und der Anzahl der Farben abhänging berechnet (Apmelgröße für alle Lichter + Textfeld für ID)
    
   const styles = {
      left: `${element.POS_X}%`, // Setze die horizontale Position basierend auf der X-Position
      top: `${element.POS_Y}%`, // Setze die vertikale Position basierend auf der Y-Position
      width: `${element.SIZE}%`, // Setze die Breite basierend auf der SIZE des Ampel-Elements
      height: `${height}%`, // Setze die Höhe basierend auf der berechneten Höhe
      display: 'flex',
      flexDirection: 'column', // Richte die inneren Elemente in einer Spalte an
      alignItems: 'center', // Richte die inneren Elemente in der Mitte aus
    };

    return styles; // Rückgabe des berechneten Styles
  }



}
