import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(private http: HttpClient, public router: Router, private titleService: Title) { }

 

  ngOnInit() {
    // Setze den HTML-Titel der Seite
    this.titleService.setTitle("Maschinenampel");
  }
}
