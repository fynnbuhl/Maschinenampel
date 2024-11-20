import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class WebSocketService {
  public socket!: WebSocket; // WebSocket-Instanz

  // connect-Methode zum Herstellen einer WebSocket-Verbindung
  connect(url: string): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      console.log('WebSocket-Verbindung bereits offen.');
      return;
    }

    this.socket = new WebSocket(url);

    this.socket.onopen = () => {
      console.log('WebSocket-Verbindung geöffnet.');
    };

    this.socket.onerror = (error) => {
      console.error('WebSocket-Fehler:', error);
    };
  }

  sendMessage(message: string): void {
    if (this.socket && this.socket.readyState === WebSocket.OPEN) {
      this.socket.send(message);
      console.log('Nachricht gesendet:', message);
    } else {
      console.error('WebSocket ist nicht offen.');
    }
  }

  closeConnection(): void {
    if (this.socket) {
      console.log('WebSocket-Verbindung geschlossen.');
      this.socket.close();
    }
  }
}
