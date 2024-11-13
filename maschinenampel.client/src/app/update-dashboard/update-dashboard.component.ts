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

  // Ein Array, in dem die abgerufenen Dashboards gespeichert werden
  Dashboards: any[] = [];

  // Der Konstruktor, der die Abhängigkeiten (HttpClient und Router) für diese Komponente injiziert
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService) { }

  // Methode zum Abrufen der Dashboards-Daten von der API
  refreshBoards() {
    // Mit HttpClient wird eine GET-Anfrage an die API gesendet, um die Dashboards-Daten zu erhalten
    this.http.get<any[]>(this.apiConfig.DB_APIUrl + 'getDashboards')
      .subscribe(data => { // Wenn die Antwort empfangen wird:
        this.Dashboards = data; // Speichert die empfangenen Daten in der Dashboards-Variable
        console.log("API Response:", data); // Gibt die empfangenen Daten zur Kontrolle in der Konsole aus
      });
  }

  // ngOnInit ist ein Angular-Lebenszyklus-Hook, der beim Initialisieren der Komponente aufgerufen wird
  // Hier wird die Methode refreshBoards aufgerufen, um die Dashboards-Daten beim Laden der Komponente zu laden
  ngOnInit() {
    this.refreshBoards();
  }
}
