import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-add-dashboard',
  templateUrl: './add-dashboard.component.html',
  styleUrl: './add-dashboard.component.css'
})
export class AddDashboardComponent implements OnInit {

  constructor(private http: HttpClient, public router: Router) { }


  // ngOnInit ist ein Angular-Lebenszyklus-Hook, der beim Initialisieren der Komponente aufgerufen wird
  // Hier wird die Methode refreshBoards aufgerufen, um die Dashboards-Daten beim Laden der Komponente zu laden
  ngOnInit() {
  }
}
