// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-update-dashboard',
  templateUrl: './update-dashboard.component.html',
  styleUrl: './update-dashboard.component.css'
})
export class UpdateDashboardComponent implements OnInit {

  selectedID: number = 0;
  selectedNAME: string = "";
  selectedIMG: string = "";

  POS_Xnew = 0;
  POS_Ynew = 0;
  SIZEnew = 4;
  colorsNew: string[] = [];
  OPC_BITnew = "";
  colorArraynew: string[] = [];
  BitArraynew: string[] = [];

  colorArray: string[] = [];
  BitArray: string[] = [];

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







  async addAmpel(Board_ID: number) {
    // Debugging: Zeige die ID des Dashboards an, in dem die Ampel hinzugefügt wird
    console.log("Board_ID:" + Board_ID);

    // OPC-Bits in ein Array umwandeln, ähnlich wie bei den Farben
    this.BitArraynew = this.OPC_BITnew.trim().split(',').filter(bit => bit.trim().length > 0);

    // Validierung der Nutzereingabe: Überprüfe, ob die Anzahl der Farben mit der Anzahl der OPC-Bits übereinstimmt
    if (this.colorsNew.length != this.BitArraynew.length) {
      console.log("Colors/OPC-Bits: Anzahl stimmt nicht überein!");
      // Zeige dem Benutzer eine verständliche Fehlermeldung an und brich die Methode ab
      alert("Anzahl der Farben stimmt nicht mit der Anzahl an OPC-Bits überein. Bitte Werte prüfen!");
      return;
    }

    if (this.colorsNew.length <= 0) {
      console.log("Bitte mindestens eine Farbe hinzufügen!");
      // Zeige dem Benutzer eine verständliche Fehlermeldung an und brich die Methode ab
      alert("Bitte mindestens eine Farbe hinzufügen!");
      return;
    }

    try {
      // Erstelle das JSON-Objekt, das an den Server gesendet wird
      const body = {
        Dashboard_ID: Board_ID,                 // ID des Dashboards, zu dem die Ampel gehört
        POS_X: this.POS_Xnew,                   // X-Position der Ampel im Dashboard
        POS_Y: this.POS_Ynew,                   // Y-Position der Ampel im Dashboard
        SIZE: this.SIZEnew,                     // Größe der Ampel
        ColorCount: this.colorsNew.length,
        COLORS: this.colorsNew.join(','),
        OPC_BIT: this.BitArraynew.join(',')     // OPC-Bits als kommagetrennter String
      };

      // Debugging: Zeige die zu sendenden Daten an
      console.log(body);

      // Sende eine POST-Anfrage an die API mit dem erstellten JSON-Objekt
      const response = await lastValueFrom(
        this.http.post(this.apiConfig.DB_APIUrl + "addAmpel", body)
      );

      // Erfolgreiche Antwort: Zurücksetzen der Eingabewerte auf Standardwerte
      this.POS_Xnew = 0;        // Standardwert für die X-Position
      this.POS_Ynew = 0;        // Standardwert für die Y-Position
      this.SIZEnew = 4;         // Standardgröße der Ampel
      this.colorsNew = [];      // Leere Eingabe für Farben
      this.selectedColor = '#000000';
      this.OPC_BITnew = "";     // Leere Eingabe für OPC-Bits

      // Debugging: Zeige die erfolgreiche Speicherung an
      console.log("Ampel erfolgreich gespeichert:", response);
    } catch (error) {
      // Fehlerbehandlung: Zeige eine Fehlermeldung im Fehlerfall
      console.error("Fehler beim Speichern der Ampel", error);
      alert("Fehler beim Speichern der Ampel. Bitte versuche es erneut.");
    }

    // Aktualisiere die Liste der Ampeln im Dashboard, um die neue Ampel anzuzeigen
    await this.getAmpelnVonBoard();
  }


  //Funktionalität des ColorPicker-Inputs
  selectedColor: string = '#000000'; // Standardfarbe
  maxColors: number = 6;

  addColor(): void {
    if (this.colorsNew.length < this.maxColors) {
      if (!this.colorsNew.includes(this.selectedColor)) {
        this.colorsNew.push(this.selectedColor);
      } else {
        alert('Diese Farbe ist bereits hinzugefügt!');
      }
    }
    console.log(this.colorsNew);
  }

  removeColor(index: number): void {
    this.colorsNew.splice(index, 1);
  }

  openColorPicker(): void {
    const colorPicker = document.getElementById('color-picker') as HTMLInputElement;
    colorPicker.click(); // Öffnet das versteckte Farbauswahlfeld
  }










  async updateAmpel(Ampel_ID: number, ampel: any) {
    // Debugging: Zeige die ID der zu aktualisierenden Ampel an
    console.log("Update ID:" + Ampel_ID);

    // Farben in ein Array umwandeln, indem der String nach Kommas getrennt wird
    this.colorArray = ampel.COLORS.trim().split(',').filter((item: string) => item !== '');

    console.log(this.colorArray);
    console.log(this.colorArray.length);

    // OPC-Bits ebenfalls in ein Array umwandeln, ähnlich wie bei den Farben
    this.BitArray = ampel.OPC_BIT.trim().split(',').filter((item: string) => item !== '');

    // Validierung der Nutzereingabe: Überprüfe, ob die Anzahl der Farben mit der Anzahl der OPC-Bits übereinstimmt
    if (this.colorArray.length != this.BitArray.length) {
      console.log("Colors/OPC-Bits: Anzahl stimmt nicht überein!");
      // Zeige dem Benutzer eine verständliche Fehlermeldung an und brich die Methode ab
      alert("Anzahl der Farben stimmt nicht mit der Anzahl an OPC-Bits überein. Bitte Werte prüfen!");
      return;
    }

    if (this.colorArray.length <= 0) {
      console.log("Bitte mindestens eine Farbe hinzufügen!");
      // Zeige dem Benutzer eine verständliche Fehlermeldung an und brich die Methode ab
      alert("Bitte mindestens eine Farbe hinzufügen!");
      return;
    }

    try {
      // Erstelle das JSON-Objekt mit den aktualisierten Werten der Ampel
      const updatedAmpel = {
        Dashboard_ID: Ampel_ID,            // ID des Dashboards, zu dem die Ampel gehört
        POS_X: ampel.POS_X,                // Aktualisierte X-Position der Ampel
        POS_Y: ampel.POS_Y,                // Aktualisierte Y-Position der Ampel
        SIZE: ampel.SIZE,                  // Aktualisierte Größe der Ampel
        ColorCount: this.colorArray.length,       // Anzahl der Farben
        COLORS: this.colorArray.join(','), // Farben als kommagetrennter String
        OPC_BIT: this.BitArray.join(',')   // OPC-Bits als kommagetrennter String
      };

      // Debugging: Zeige das zu sendende Objekt an
      console.log(updatedAmpel);

      // Sende eine POST-Anfrage an die API, um die Ampel zu aktualisieren
      const response = await lastValueFrom(
        this.http.post(this.apiConfig.DB_APIUrl + "updateAmpel", updatedAmpel)
      );

      // Erfolgreiche Antwort


      // Debugging: Zeige die erfolgreiche Aktualisierung an
      console.log("Ampel erfolgreich gespeichert:", response);
    } catch (error) {
      // Fehlerbehandlung: Zeige eine Fehlermeldung im Fehlerfall
      console.error("Fehler beim Speichern der Ampel", error);
      alert("Fehler beim Speichern der Ampel. Bitte versuche es erneut.");
    }

    // Aktualisiere die Liste der Ampeln im Dashboard, um die Änderungen anzuzeigen
    await this.getAmpelnVonBoard();
  }



  async deleteAmpel(ID: number) {
    console.log("Lösche ampel mit ID:" + ID);

    const isConfirmed = window.confirm("Möchten Sie diese Ampel wirklich löschen?");
    if (isConfirmed) {
      // Mit HttpClient wird eine POST-Anfrage an die API gesendet, um die Ampel-Daten zu löschen
      const res = await lastValueFrom(
        this.http.post(`${this.apiConfig.DB_APIUrl}deleteAmpel?ID=${ID}`, {})
      );
      console.log("API Response:", res); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus

      await this.getAmpelnVonBoard();
    }
  }






  async updateBoard(ID: number) {
    console.log("Update ID:" + ID);

    const bodyBoards = {
      NewName: this.selectedNAME // Einfacher JSON-Body
    };

    console.log(this.selectedNAME);

    // Sende eine POST-Anfrage an die API, um das Dashboard zu aktualisieren 
    try {
      // Sende die POST-Anfrage und warte auf die Antwort
      const res = await lastValueFrom(
        this.http.post(`${this.apiConfig.DB_APIUrl}updateDashboardName?ID=${ID}`, bodyBoards)
      );

      console.log("Dashboard erfolgreich aktualisiert.");
      alert("Dashboard erfolgreich aktualisiert.");
    } catch (error) {
      // Fehlerbehandlung
      console.error("Fehler beim Speichern des Dashboards", error);
      alert("Fehler beim Speichern des Dashboards. Bitte versuche es erneut.");
    }

  }



  async deleteBoard(ID: number) {
    console.log("Lösche ID:" + ID);

    const isConfirmed = window.confirm("Möchten Sie dieses Dashboard wirklich löschen?");
    if (isConfirmed) {
      // Mit HttpClient wird eine POST-Anfrage an die API gesendet, um die Dashboard-Daten zu löschen (inkl. Ampeln)
      const res = await lastValueFrom(
        this.http.post(`${this.apiConfig.DB_APIUrl}deleteDashboard?ID=${ID}`, {})
      );
      console.log("API Response:", res); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus
        
      await this.clearID(); //Daten Löschen
      await this.refreshBoards(); //und zurück ins Menu gehen
    }
  }








  
}
