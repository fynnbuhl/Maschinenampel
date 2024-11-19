// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten

@Component({
  selector: 'app-update-dashboard',
  templateUrl: './update-dashboard.component.html',
  styleUrl: './update-dashboard.component.css'
})
export class UpdateDashboardComponent implements OnInit {

  selectedID: number = 0;
  selectedNAME: string = "";
  selectedIMG: string = "";

  POS_X = "";
  POS_Y = "";
  SIZE = "";

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
    console.log("Board_ID:" + Board_ID);


    await this.getAmpelnVonBoard();
  }

  updateAmpel(ID: number) {
    console.log("Update ID:" + ID);
    
  }

  async deleteAmpel(ID: number) {
    console.log("Lösche ampel mit ID:" + ID);

    const isConfirmed = window.confirm("Möchten Sie diese Ampel wirklich löschen?");
    if (isConfirmed) {
      // Mit HttpClient wird eine POST-Anfrage an die API gesendet, um die Ampel-Daten zu löschen
      const res = await this.http.post(`${this.apiConfig.DB_APIUrl}deleteAmpel?ID=${ID}`, {}).toPromise();
      console.log("API Response:", res); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus

      await this.getAmpelnVonBoard();
    }
  }

  updateBoard(ID: number) {
    console.log("Update ID:" + ID);

    // Ein neues FormData-Objekt wird erstellt, um die Daten zu verpacken,
    // die an den Server gesendet werden
    let bodyBoards = new FormData();

    // Füge die Daten dem FormData-Objekt hinzu
    bodyBoards.append('NewName', this.selectedNAME);

    console.log(this.selectedNAME);

    // Sende eine POST-Anfrage an die API, um das Dashboard zu aktualisieren 
    this.http.post(`${this.apiConfig.DB_APIUrl}updateDashboardName?ID=${ID}`, bodyBoards)
      .subscribe(
        (res) => {
          // Bei erfolgreicher Antwort
          console.log("Dashboard erfolgreich aktualisiert.");
          alert("Dashboard erfolgreich aktualisiert.");
        },
        (error) => {
          // Fehlerbehandlung: Zeige eine Fehlermeldung, falls die Anfrage fehlschlägt
          console.error("Fehler beim Speichern des Dashboards", error);
          alert("Fehler beim Speichern des Dashboards. Bitte versuche es erneut.");
        }
      );

  }

  async deleteBoard(ID: number) {
    console.log("Lösche ID:" + ID);

    const isConfirmed = window.confirm("Möchten Sie dieses Dashboard wirklich löschen?");
    if (isConfirmed) {
      // Mit HttpClient wird eine POST-Anfrage an die API gesendet, um die Dashboard-Daten zu löschen (inkl. Ampeln)
      const res = await this.http.post(`${this.apiConfig.DB_APIUrl}deleteDashboard?ID=${ID}`, {}).toPromise();
      console.log("API Response:", res); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus
        
      await this.clearID(); //Daten Löschen
      await this.refreshBoards(); //und zurück ins Menu gehen
    }
  }


}
