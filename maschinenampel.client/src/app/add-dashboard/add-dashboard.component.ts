// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten

import { Observable } from 'rxjs'; // RxJS (Reactive Extensions for JavaScript) wird für asynchrone Operationen und reaktive Programmierung verwendet.
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root' // Gibt an, dass der Service in der gesamten Anwendung verfügbar ist.
  // Der Service wird im Root-Injektor registriert. Dadurch wird sichergestellt, dass es eine einzige Instanz des Services gibt (Singleton).
})



@Component({
  selector: 'app-add-dashboard',
  templateUrl: './add-dashboard.component.html',
  styleUrls: ['./add-dashboard.component.css']
})
export class AddDashboardComponent implements OnInit {

  ngOnInit() {

  }

  // Properties für die Werte des neuen Dashboards
  newBoard = ""; // Name des neuen Dashboards
  IMG_PATH = ""; // Pfad oder URL des Bildes für das Dashboard#
  
  // Der Konstruktor injiziert HttpClient, um HTTP-Anfragen zu senden
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService) { }





  // Methode, um ein neues "Board" hinzuzufügen
  async add_board() {
    try {
      // Warten, bis die Datei hochgeladen wurde, und die URL speichern
      this.IMG_PATH = await this.uploadFile();
      this.IMG_PATH = this.IMG_PATH.replace(/\\/g, '/'); // Ersetze Rückwärtsschrägstriche durch Schrägstriche
      console.log(this.IMG_PATH); // Ausgabe der hochgeladenen URL zur Überprüfung

      // Speichere die neuen Daten in der Datenbank
      await this.saveToDB();
      await window.location.reload(); //Komponente neu laden
      console.log("Neues Board erfolgreich hinzugefügt.");

      // Benutzerfeedback: Bestätigung, dass das Board erfolgreich gespeichert wurde
      alert("Neues Dashboard erfolgreich gespeichert.");
    } catch (error) {
      // Fehlerbehandlung: Ausgabe einer Fehlermeldung in der Konsole
      console.error("Fehler beim Hochladen oder Speichern:", error);
    }
  }




  // Variable, um die ausgewählte Datei zu speichern
  selectedFile: File | null = null; // `null`, wenn keine Datei ausgewählt ist
  uploadResponse: string | null = null; // Nachricht über den Status des Uploads

  // Methode, die aufgerufen wird, wenn eine Datei über das File-Input-Element ausgewählt wird
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement; // Event wird in ein HTML-Input-Element umgewandelt
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0]; // Die erste ausgewählte Datei wird gespeichert
    }
  }

  // Methode, die den Upload-Prozess ausführt und die URL der hochgeladenen Datei zurückgibt
  uploadFile(): Promise<string> {
    return new Promise((resolve, reject) => {
      // Überprüfen, ob eine Datei ausgewählt wurde
      if (!this.selectedFile) {
        this.uploadResponse = "Bitte wählen Sie eine Datei aus."; // Feedback an den Benutzer
        reject("Keine Datei ausgewählt"); // Promise wird mit Fehler abgelehnt
        return;
      }

      // Datei an den Server senden
      this.uploadFileToServer(this.selectedFile).subscribe({
        next: (response) => {
          // Wenn der Server eine gültige URL zurückgibt
          if (response.URL) {
            resolve(response.URL); // Promise mit der URL auflösen
          } else {
            reject("Keine gültige URL in der Antwort"); // Promise mit Fehler ablehnen
          }
        },
        error: () => {
          // Fehler beim Hochladen
          this.uploadResponse = "Fehler beim Hochladen der Datei.";
          reject("Upload fehlgeschlagen"); // Promise mit Fehler ablehnen
        }
      });
    });
  }

  // Private Methode, um die Datei an den Server zu senden
  private uploadFileToServer(file: File): Observable<any> {
    const formData = new FormData(); // Erstelle ein FormData-Objekt für den Datei-Upload
    formData.append('file', file); // Füge die Datei zum FormData hinzu

    // Sende das FormData per POST-Anfrage an den Server
    return this.http.post(this.apiConfig.uploadUrl, formData);
  }





  // Methode zum Speichern eines neuen Dashboards in der Datenbank
  saveToDB() {

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
          // Bei erfolgreicher Antwort:
          this.newBoard = ""; // Setze das Eingabefeld für den Namen zurück
          this.IMG_PATH = ""; // Setze das Eingabefeld für den IMG_PATH zurück
        },
        (error) => {
          // Fehlerbehandlung: Zeige eine Fehlermeldung, falls die Anfrage fehlschlägt
          console.error("Fehler beim Speichern des Dashboards", error);
          alert("Fehler beim Speichern des Dashboards. Bitte versuche es erneut.");
        }
      );
  }



}
