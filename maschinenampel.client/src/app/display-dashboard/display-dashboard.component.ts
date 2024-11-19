// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten

// Deklaration der Komponente mit Metadaten
@Component({
  selector: 'app-display-dashboard', // Der Selector ist der Name, unter dem die Komponente im HTML verwendet wird
  templateUrl: './display-dashboard.component.html', // Das Template (HTML) für diese Komponente
  styleUrl: './display-dashboard.component.css' // Das CSS für die Gestaltung dieser Komponente
})
export class DisplayDashboardComponent implements OnInit {

  selectedID: number = 0;
  selectedNAME: string = "";
  selectedIMG: string = "";

  // Ein Array, in dem die abgerufenen Dashboards/Ampeln gespeichert werden
  Dashboards: any[] = [];
  Ampeln: any[] = [];

  // Der Konstruktor, der die Abhängigkeiten (HttpClient und Router) für diese Komponente injiziert
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService) { }

  // ngOnInit ist ein Angular-Lebenszyklus-Hook, der beim Initialisieren der Komponente aufgerufen wird
  // Hier wird die Methode refreshBoards aufgerufen, um die Dashboards-Daten beim Laden der Komponente zu laden
  ngOnInit() {
    this.refreshBoards();
  }


  // Methode zum Abrufen der Dashboards-Daten von der API
  refreshBoards() {
    // Mit HttpClient wird eine GET-Anfrage an die API gesendet, um die Dashboards-Daten zu erhalten
    this.http.get<any[]>(this.apiConfig.DB_APIUrl + 'getDashboards')
      .subscribe(data => { // Wenn die Antwort empfangen wird:
        this.Dashboards = data; // Speichert die empfangenen Daten in der Dashboards-Variable
        //console.log("API Response:", data); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus
      });
  }


  viewBoard(ID: number, NAME: string, IMG_PATH: string): void {
    this.selectedID = ID;
    this.selectedNAME = NAME;
    this.selectedIMG = IMG_PATH;
    console.log("Ausgewählte Dashboard-ID:" + this.selectedID);

    this.refreshBoards();
    this.getAmpelnVonBoard();
  }


  clearID(): void {
    this.selectedID = 0;
    this.selectedNAME = "";
    this.selectedIMG = "";
  }


  // Methode zum Abrufen der Ampel-Daten von der API
  getAmpelnVonBoard() {
    // Mit HttpClient wird eine GET-Anfrage an die API gesendet, um die Ampel-Daten zu erhalten
    this.http.get<any[]>(`${this.apiConfig.DB_APIUrl}getAmpeln?selected_ID=${this.selectedID}`)
      .subscribe(data => { // Wenn die Antwort empfangen wird:
        this.Ampeln = data; // Speichert die empfangenen Daten in der Ampel-Variable
        //console.log("API Response:", data); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus
      });
  }



  // Methode, um die Styles für jedes Ampel-Element zu berechnen
  getElementStyles(element: any) {
    const height = element.ColorCount * element.SIZE * 0.66;

    // Berechnung der Position und Größe für das Ampel-Element
    const styles = {
      left: `${element.POS_X}%`,
      top: `${element.POS_Y}%`,
      width: `${element.SIZE}px`,
      height: `${height}px`,
      display: 'flex',
      flexDirection: 'column',
      justifyContent: 'space-between',
      alignItems: 'center'
    };

    return styles;
  }




  
}
