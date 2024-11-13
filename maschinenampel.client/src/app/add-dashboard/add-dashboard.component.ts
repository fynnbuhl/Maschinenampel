// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten

@Component({
  selector: 'app-add-dashboard',
  templateUrl: './add-dashboard.component.html',
  styleUrls: ['./add-dashboard.component.css']
})
export class AddDashboardComponent {

  // Properties für die Werte des neuen Dashboards
  newBoard = ""; // Name des neuen Dashboards
  IMG_PATH = ""; // Pfad oder URL des Bildes für das Dashboard

  // Der Konstruktor injiziert HttpClient, um HTTP-Anfragen zu senden
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService) { }

  // Methode zum Hinzufügen eines neuen Dashboards
  add_board() {
    // Ein neues FormData-Objekt wird erstellt, um die Daten zu verpacken,
    // die an den Server gesendet werden
    let bodyBoards = new FormData();

    // Füge die Daten dem FormData-Objekt hinzu, indem der Name und der IMG_PATH
    // des neuen Dashboards übergeben werden
    bodyBoards.append('Name', this.newBoard);
    bodyBoards.append('IMG_PATH', this.IMG_PATH);

    // Sende eine POST-Anfrage an die API, um das neue Dashboard zu erstellen
    this.http.post(this.apiConfig.DB_APIUrl + "addDashboard", bodyBoards)
      .subscribe(
        (res) => {
          // Bei erfolgreicher Antwort wird der Benutzer informiert und die Felder zurückgesetzt
          alert("Board wurde hinzugefügt."); // Zeige die Serverantwort in einem Alert-Fenster
          this.newBoard = ""; // Setze das Eingabefeld für den Namen zurück
          this.IMG_PATH = ""; // Setze das Eingabefeld für den IMG_PATH zurück
        },
        (error) => {
          // Fehlerbehandlung: Zeige eine Fehlermeldung, falls die Anfrage fehlschlägt
          console.error("Fehler beim Hinzufügen des Dashboards", error);
          alert("Fehler beim Hinzufügen des Dashboards. Bitte versuche es erneut.");
        }
      );
  }
}
