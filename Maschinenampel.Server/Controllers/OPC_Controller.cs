//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC
using Newtonsoft.Json;
using System.Data;
using System.Net.WebSockets;
using System.Text;
using static Maschinenampel.Server.Controllers.OPC_Controller;

namespace Maschinenampel.Server.Controllers
{
    
    [Route("api/OPCController")] // Definiert die Route für diesen Controller
    [ApiController]
    [RequireHttps]
    public class OPC_Controller : ControllerBase
    {
        public string[][] OPC_getBitFromAddr = new string[0][];

        public class OPCModel
        {
            //public string[,] OPC_BIT_Addr { get; set; } //z.B. { { "A1", "A2" }, { "B1", "B2", "B3" } }
            public string[][] OPC_BIT_Addr { get; set; }
        }

        [HttpPost]
        [Route("getBITs")]
        public async Task<IActionResult> GetBits([FromBody] OPCModel model)
        {
            if (model == null)
            {
                return BadRequest("Ungültige Eingabedaten");
            }

            // Speichern einer Kopie der OPC_BIT_Addr
            OPC_getBitFromAddr = model.OPC_BIT_Addr;

            //Testausgabe
            Console.WriteLine("Empfangene Adressen:");
            foreach (var row in OPC_getBitFromAddr)
            {
                Console.WriteLine(string.Join(", ", row));
            }


            try
            {
                // Beispiel für dynamische Erstellung eines BIT Arrays.
                // Hier könnte eine asynchrone Methode zum Abrufen der BIT-Werte von einem OPC-Server integriert werden.
                int[][] OPC_BITArray = await GetOPCBitsFromControllerAsync(OPC_getBitFromAddr);

                if (OPC_BITArray == null || OPC_BITArray.Length == 0)
                {
                    return NotFound("Keine BIT-Werte für die angegebene Adressen gefunden.");
                }

                return Ok(OPC_BITArray);
            }
            catch (Exception ex)
            {
                // Fehlerprotokollierung
                Console.WriteLine($"Fehler beim Abrufen der BITs: {ex.Message}");
                return StatusCode(500, "Es gab ein Problem beim Verarbeiten der Anfrage.");
            }
        }




        // Beispielmethoden zum Abrufen von OPC-BIT-Werten (als asynchrone Methode)
        private async Task<int[][]> GetOPCBitsFromControllerAsync(string[][] opcAddressArray)
        {
            // Simulieren eines asynchronen Abrufs der Daten
            await Task.Delay(500);

            // Beispielantwort
            return new int[][]
            {
                new int[] { 1, 0 },   // Erster Array: [1, 0]
                new int[] { 0, 1, 0 }  // Zweiter Array: [0, 1, 0]
            };
        }











        // Diese Methode wird durch eine GET-Anfrage an 'api/websocket/connectWebSocket' aufgerufen.
        // Sie stellt die WebSocket-Verbindung her.
        [HttpGet("connectWebSocket")]
        public async Task ConnectWebSocket()
        {

            // Überprüfen, ob die Anfrage eine WebSocket-Anfrage ist
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                // Akzeptiere die WebSocket-Verbindung
                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                // Beginne mit der Handhabung der WebSocket-Verbindung
                await HandleWebSocketAsync(webSocket);
            }
            else
            {
                // Wenn es keine WebSocket-Anfrage ist, sende den Statuscode 400 (Bad Request)
                HttpContext.Response.StatusCode = 400;
            }
        }

        // Diese Methode verarbeitet die WebSocket-Verbindung und sendet ständig aktualisierte Daten an den Client
        private async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4]; // Puffergröße für empfangene Daten (4 KB)

            // Initialisiere das 2D-Array mit Beispieldaten
            int[][] array = new int[][]
            {
                new int[] { 1, 1 },  // Erste Zeile des 2D-Arrays
                new int[] { 0, 1, 1 } // Zweite Zeile des 2D-Arrays
            };


            // Empfange Client-Nachrichten in einem separaten Task, um die regelmäßige Array-Aktualisierung nicht zu blockieren.
            var receiveTask = Task.Run(async () =>
            {
                
                try
                {
                    while (webSocket.State == WebSocketState.Open)
                    {
                        // Empfange Daten, um zu prüfen, ob der Client ein Close-Frame sendet
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.CloseStatus.HasValue)
                        {
                            Console.WriteLine($"WebSocket vom Client geschlossen: {result}");
                            break;
                        }

                        // Hier könntest du empfangene Daten verarbeiten, falls nötig
                        Console.WriteLine("Nachricht vom Client empfangen.");
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"WebSocket-Fehler: {ex.Message}");
                }
            });



            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    
                    array = UpdateArray(array);// Aktualisiere das Array regelmäßig

                    string json = JsonConvert.SerializeObject(array); // Wandle das Array in einen JSON-String um, um es an den Client zu senden
                    var bytes = Encoding.UTF8.GetBytes(json); // Wandle den JSON-String in Bytes um (weil WebSockets nur Binärdaten übertragen können)

                    // Sende die Bytes an den Client
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );

                    // Verzögerung von 1 Sekunden, bevor der Vorgang wiederholt wird
                    // Das ermöglicht es, dass der Client alle 1 Sekunden neue Daten empfängt
                    await Task.Delay(1000);
                }
            }
            finally
            {
                await receiveTask; // Stelle sicher, dass der Empfangs-Task beendet wird
                OPC_getBitFromAddr = new string[0][];
            }


        }

        /*private async Task HandleWebSocketAsync(WebSocket webSocket)
        {
            // Puffergröße für empfangene Daten (4 KB)
            byte[] buffer = new byte[1024 * 4];

            // Initialisiere das 2D-Array mit Beispieldaten
            int[][] array = new int[][]
            {
                new int[] { 1, 1 },  // Erste Zeile des 2D-Arrays
                new int[] { 0, 1, 1 } // Zweite Zeile des 2D-Arrays
            };

            // Solange die WebSocket-Verbindung offen ist, wird diese Schleife immer wieder ausgeführt
            while (webSocket.State == WebSocketState.Open)
            {

                // Aktualisiere das Array mit neuen Werten
                array = UpdateArray(array);

                // Wandle das Array in einen JSON-String um, um es an den Client zu senden
                string json = JsonConvert.SerializeObject(array);

                // Wandle den JSON-String in Bytes um (weil WebSockets nur Binärdaten übertragen können)
                var bytes = Encoding.UTF8.GetBytes(json);

                // Sende die Bytes an den Client
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);

                // Verzögerung von 1 Sekunden, bevor der Vorgang wiederholt wird
                // Das ermöglicht es, dass der Client alle 1 Sekunden neue Daten empfängt
                await Task.Delay(1000);
            }



            OPC_getBitFromAddr = []; //relevante Addressen zurücksetzten
            Console.WriteLine("WebSocket wurde geschlossen.");
        }*/



        // Diese Methode wird verwendet, um das Array zufällig zu ändern
        private int[][] UpdateArray(int[][] array)
        {
            // Erstelle eine Instanz des Random-Objekts, um Zufallszahlen zu generieren
            var rand = new Random();

            // Iteriere durch das 2D-Array
            for (int i = 0; i < array.Length; i++)
            {
                // Iteriere durch jede Zeile des Arrays
                for (int j = 0; j < array[i].Length; j++)
                {
                    // 50% Wahrscheinlichkeit, den Wert zu ändern
                    // Wenn der Zufallswert 0 oder 1 ist, dann wird das aktuelle Element geändert
                    if (rand.Next(2) == 0)
                    {
                        // Ändere den Wert von 1 auf 0 und von 0 auf 1
                        // Dies erfolgt durch eine einfache Bedingung
                        array[i][j] = array[i][j] == 1 ? 0 : 1;
                    }
                }
            }

            // Gebe das aktualisierte Array zurück
            return array;
        }











    }
}
