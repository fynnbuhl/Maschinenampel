import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {

  // Titel der App
  title = 'Maschinenampel';

  // API-URL für den Endpunkt, der die Dashboards zurückgibt
  readonly APIUrl = "https://localhost:7204/api/DBController/";

  // Variable zur Speicherung der empfangenen Dashboards (JSON-Daten)
  Dashboards: any[] = [];

  constructor(private http: HttpClient) { }

  // Methode, die die Daten abruft
  refreshBoards() {
    this.http.get<any[]>(this.APIUrl + 'getDashboards')
      .subscribe(data => {
        this.Dashboards = data; // Speichert die empfangenen JSON-Daten
        console.log("API Response:", data); // Logge die empfangenen Daten
      });
  }

  // Initialisierung: ruft die Daten beim Laden der Komponente ab
  ngOnInit() {
    this.refreshBoards();
  }
}
