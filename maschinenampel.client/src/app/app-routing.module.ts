// Importiere die erforderlichen Angular-Module
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Importiere die Komponenten, die den einzelnen Routen zugeordnet werden
import { DisplayDashboardComponent } from './display-dashboard/display-dashboard.component';
import { AddDashboardComponent } from './add-dashboard/add-dashboard.component';
import { UpdateDashboardComponent } from './update-dashboard/update-dashboard.component';

// Definiere die Routen-Konfiguration für die Anwendung
// Jede Route entspricht einem bestimmten Pfad (URL) und einer Komponente, die geladen wird, wenn die Route aufgerufen wird
const routes: Routes = [
  // Route für die Seite "Display Dashboard" 
  // Bei der URL '/displayDashboard' wird DisplayDashboardComponent angezeigt
  { path: 'displayDashboard', component: DisplayDashboardComponent },

  // Route für die Seite "Add Dashboard"
  // Bei der URL '/addDashboard' wird AddDashboardComponent angezeigt
  { path: 'addDashboard', component: AddDashboardComponent },

  // Route für die Seite "Update Dashboard"
  // Bei der URL '/updateDashboard' wird UpdateDashboardComponent angezeigt
  { path: 'updateDashboard', component: UpdateDashboardComponent }
];

// Deklariere und konfiguriere das Routing-Modul
@NgModule({
  // Importiere das RouterModule und konfiguriere es mit den definierten Routen
  // forRoot() wird verwendet, um die Routen nur einmal in der gesamten Anwendung zu registrieren
  imports: [RouterModule.forRoot(routes)],

  // Exportiere das RouterModule, damit es in anderen Modulen, die AppRoutingModule importieren, verfügbar ist
  exports: [RouterModule]
})
// Exportiere die Klasse AppRoutingModule, die das Routing für die Anwendung verwaltet
export class AppRoutingModule { }
