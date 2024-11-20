// Importiere die notwendigen Angular-Module und -Services
import { HttpClient } from '@angular/common/http'; // HttpClient wird verwendet, um HTTP-Anfragen zu machen
import { Component, OnInit } from '@angular/core'; // Component und OnInit werden benötigt, um eine Angular-Komponente zu definieren
import { Router } from '@angular/router'; // Router wird verwendet, um zur Navigation zwischen verschiedenen Routen zu ermöglichen
import { ApiConfigService } from '@service/API_Service'; //ApiConfigService wird verwendet um die API-URLs global zu verwalten
import { lastValueFrom } from 'rxjs';

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
  IMG_PATH = ""; // Pfad oder URL des Bildes für das Dashboard
  aspectRatio = ""; //aspectRatio vom Bild

  loading: boolean = false; // Variable für den Ladezustand
  
  // Der Konstruktor injiziert HttpClient, um HTTP-Anfragen zu senden
  constructor(private http: HttpClient, public router: Router, private apiConfig: ApiConfigService) { }





  // Methode zum Hinzufügen eines neuen "Boards"
  // Die Methode ist asynchron, da sie auf den Abschluss von Dateiuploads und Datenbankoperationen wartet.
  async add_board() {
    try {
      // Aktiviert den Lade-Indikator (z.B. Spinner oder ähnliches), um dem Benutzer zu zeigen, dass der Vorgang läuft
      this.loading = true;

      // Wartet auf den Abschluss des Datei-Uploads und speichert den Pfad der hochgeladenen Datei in der Variable 'IMG_PATH'
      // Die 'uploadFile()'-Methode wird vermutlich die Datei hochladen und die URL des hochgeladenen Bildes zurückgeben
      this.IMG_PATH = await this.uploadFile();

      // Ersetzt alle Rückwärtsschrägstriche ('\') durch Schrägstriche ('/'), um sicherzustellen, dass der Pfad mit dem üblichen
      // Web-Pfadformat übereinstimmt (z.B. für URLs oder Dateisysteme, die Schrägstriche verwenden)
      this.IMG_PATH = this.IMG_PATH.replace(/\\/g, '/');

      // Ausgabe des Pfades des hochgeladenen Bildes zur Überprüfung
      //console.log(this.IMG_PATH);

      // Ruft die Methode 'saveToDB()' auf, die die neuen Daten (inklusive des Bildpfads) in der Datenbank speichert
      await this.saveToDB();

      // Nach erfolgreichem Speichern wird die Seite neu geladen, um die Änderungen anzuzeigen
      await window.location.reload();

      // Ausgabe in der Konsole, dass das neue Board erfolgreich hinzugefügt wurde
      console.log("Neues Board erfolgreich hinzugefügt.");

      // Deaktiviert den Lade-Indikator, da der Vorgang abgeschlossen ist
      this.loading = false;

      // Zeigt dem Benutzer eine Bestätigung an, dass das Dashboard erfolgreich gespeichert wurde
      alert("Neues Dashboard erfolgreich gespeichert.");

    } catch (error) {
      // Fehlerbehandlung: Wenn ein Fehler auftritt, wird eine Fehlermeldung in der Konsole ausgegeben
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
  // Die Methode gibt ein Promise zurück, das entweder die URL der hochgeladenen Datei oder eine Fehlermeldung liefert.
  uploadFile(): Promise<string> {
    return new Promise((resolve, reject) => {
      // Überprüfen, ob eine Datei ausgewählt wurde
      // Falls keine Datei ausgewählt wurde, wird der Benutzer darauf hingewiesen und das Promise wird mit einem Fehler abgelehnt
      if (!this.selectedFile) {
        this.uploadResponse = "Bitte wählen Sie eine Datei aus."; // Feedback an den Benutzer, dass keine Datei ausgewählt wurde
        reject("Keine Datei ausgewählt"); // Promise wird mit einem Fehler abgelehnt, weil keine Datei vorhanden ist
        return; // Früher Abbruch der Methode, wenn keine Datei ausgewählt wurde
      }

      // Wenn eine Datei ausgewählt wurde, wird der Upload-Prozess fortgesetzt.
      // Die Methode 'uploadFileToServer()' wird aufgerufen, um die Datei an den Server zu senden.
      // Sie gibt ein Observable zurück, auf das mit der Methode 'subscribe' reagiert wird.
      this.uploadFileToServer(this.selectedFile).subscribe({
        next: (response) => {
          // Die 'response' enthält die Informationen über den Upload: die URL der hochgeladenen Datei und das Seitenverhältnis.
          this.aspectRatio = response.aspectRatio; // Speichern des Seitenverhältnisses des hochgeladenen Bildes zur späteren Verwendung

          // Überprüfen, ob die Antwort des Servers eine gültige URL und ein Seitenverhältnis enthält.
          // Falls beide vorhanden sind, wird die URL als Ergebnis zurückgegeben.
          if (response.URL && response.aspectRatio) {
            resolve(response.URL); // Wenn eine gültige URL zurückgegeben wird, löst das Promise die URL auf.
          } else {
            // Wenn die Antwort keine gültige URL oder kein Seitenverhältnis enthält, wird das Promise mit einem Fehler abgelehnt.
            reject("Keine gültige URL in der Antwort"); // Fehlerhafte Antwort des Servers
          }
        },
        error: () => {
          // Falls beim Hochladen der Datei ein Fehler auftritt, wird diese Fehlerbehandlung aktiviert.
          // Hier kann es zu Fehlern kommen, z.B. durch Netzwerkprobleme oder Serverfehler.
          this.uploadResponse = "Fehler beim Hochladen der Datei."; // Benutzerfreundliche Fehlermeldung
          reject("Upload fehlgeschlagen"); // Das Promise wird mit einem Fehler abgelehnt, der den Upload-Prozess beschreibt
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
  async saveToDB() {

    // Erstelle das Objekt mit den zu sendenden Daten
    const bodyBoards = {
      Name: this.newBoard,
      IMG_PATH: this.IMG_PATH,
      aspectRatio: this.aspectRatio
    };

    try {
      // Sende die POST-Anfrage und warte auf die Antwort
      await lastValueFrom(this.http.post(this.apiConfig.DB_APIUrl + "addDashboard", bodyBoards));

      // Erfolgreiche Antwort: Setze die Eingabefelder zurück
      this.newBoard = "";
      this.IMG_PATH = "";
      this.aspectRatio = "";

      console.log("Dashboard erfolgreich hinzugefügt!");
    } catch (error) {
      // Fehlerbehandlung: Zeige eine Fehlermeldung, falls die Anfrage fehlschlägt
      console.error("Fehler beim Speichern des Dashboards", error);
      alert("Fehler beim Speichern des Dashboards. Bitte versuche es erneut.");
    }
  }



}
