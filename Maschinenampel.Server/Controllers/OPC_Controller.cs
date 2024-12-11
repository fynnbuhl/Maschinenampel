//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Maschinenampel.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;


namespace Maschinenampel.Server.Controllers
{
    
    [Route("api/OPCController")] // Definiert die Route für diesen Controller
    [ApiController]
    [RequireHttps]
    public class OPC_Controller : ControllerBase
    {

        string[][] OPC_AddrArray = new string[0][];
        int[][] OPC_BitArray;



        // Diese Methode wird durch eine GET-Anfrage an 'api/websocket/connectWebSocket' aufgerufen.
        // Sie stellt die WebSocket-Verbindung her.
        [HttpGet("connectWebSocket")]
        public async Task ConnectWebSocket()
        {

            // Hole den URL-Parameter, der das Address-Array enthält
            var data = HttpContext.Request.Query["addresses"].ToString();

            if (!string.IsNullOrEmpty(data))
            {
                try
                {
                    // Deserialisiere das Address-Array von JSON
                    OPC_AddrArray = JsonConvert.DeserializeObject<string[][]>(data);

                    // Initialisiere das Bit-Array
                    OPC_BitArray = new int[OPC_AddrArray.Length][];
                    for (int i = 0; i < OPC_AddrArray.Length; i++)
                    {
                        OPC_BitArray[i] = new int[OPC_AddrArray[i].Length];
                        for (int j = 0; j < OPC_AddrArray[i].Length; j++)
                        {
                            OPC_BitArray[i][j] = -1; // Setze jeden Wert auf -1 zum Debuggen
                        }
                    }

                    // Testausgabe
                    Console.WriteLine("Empfangene Adressen:");
                    foreach (var row in OPC_AddrArray)
                    {
                        Console.WriteLine(string.Join(", ", row));
                    }

                    if (OPC_BitArray != null)
                    {
                        /*Console.WriteLine("Initalisierte Bits:");
                        foreach (var row in OPC_BitArray)
                        {
                            Console.WriteLine(string.Join(", ", row));
                        }*/
                    } else 
                    {
                        Console.WriteLine("OPC_BitArray ist noch nicht initialisiert.");
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Deserialisieren des Address-Arrays: " + ex.Message);
                }
            }



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
                        // Diese Methode wird verwendet, um die Adressen in Bit-zustände zu übersetzten
                        OPC_BitArray = await UpdateBits(OPC_AddrArray, OPC_BitArray);

                        //DEBUG ONLY
                        /*Console.WriteLine("neue Bits:");
                        foreach (var row in OPC_BitArray)
                        {
                            Console.WriteLine(string.Join(", ", row));
                        }*/
                    
                        string json = JsonConvert.SerializeObject(OPC_BitArray); // Wandle das aktuelle BitArray in einen JSON-String um, um es an den Client zu senden
                        var bytes = Encoding.UTF8.GetBytes(json); // Wandle den JSON-String in Bytes um (weil WebSockets nur Binärdaten übertragen können)


                        // Sende die Bytes an den Client
                        await webSocket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None
                        );
                    

                    // Verzögerung von 0,5 Sekunden, bevor der Vorgang wiederholt wird
                    // Das ermöglicht es, dass der Client alle 0,5 Sekunden neue Daten empfängt
                    await Task.Delay(500);
                }
            }
            finally
            {
                await receiveTask; // Stelle sicher, dass der Empfangs-Task beendet wird
                OPC_AddrArray = new string[0][];
            }


        }

        



        // Diese Methode wird verwendet, um jeder übergebenen Adresse den aktuellen Bitzustand zuzuweisen
        private async Task<int [][]> UpdateBits(string[][] OPC_Addr, int[][] OPC_Bits)
        {
                                // Erstelle eine Instanz des Random-Objekts, um Zufallszahlen zu generieren
                                //var rand = new Random();

            // Iteriere durch das 2D-Array
            for (int i = 0; i < OPC_Addr.Length; i++)
            {
                // Iteriere durch jede Zeile des Arrays
                for (int j = 0; j < OPC_Addr[i].Length; j++)
                {

                                //Löschen
                                // 50% Wahrscheinlichkeit, den Wert zu ändern
                                // Wenn der Zufallswert 0 oder 1 ist, dann wird das aktuelle Element geändert
                                /*if (rand.Next(2) == 0)
                                {
                                    // Ändere den Wert von 1 auf 0 und von 0 auf 1
                                    // Dies erfolgt durch eine einfache Bedingung
                                    OPC_Bits[i][j] = OPC_Bits[i][j] == 1 ? 0 : 1;
                        
                                }*/



                    //DEBUG ONLY
                    //Console.WriteLine($"POS: {i},{j}, Addr: {OPC_Addr[i][j]}");

                    OPC_Bits[i][j] = await ReadNode(OPC_Addr[i][j]);
                }
            }

            return OPC_Bits;
        }







        private readonly OPC_Service _opcService; // Service für das Abrufen von OPC-Daten

        // Konstruktor der Klasse, der den OPC_Service für den Zugriff auf die OPC-Daten injiziert
        public OPC_Controller(OPC_Service opcService)
        {
            _opcService = opcService;
        }


        // Diese Methode gibt den Wert des OPC-Nodes als Integer zurück (0 für false, 1 für true).
        private async Task<int> ReadNode(string nodeName)
        {
            try
            {
                // Ruft die Methode ReadNodeAsync aus dem OPC-Service auf, um den aktuellen Wert des OPC-Knotens zu lesen.
                // Der Wert wird als Boolean (true/false) gespeichert.
                bool OPC_dataValue = await _opcService.ReadNodeAsync(nodeName);

                Console.WriteLine(OPC_dataValue);

                // Wandelt den booleschen Wert in einen Integer um und gibt ihn zurück
                return OPC_dataValue ? 1 : 0;
            }
            catch (Exception ex)
            {
                // Wenn ein Fehler auftritt (z. B. Verbindungsprobleme oder ungültiger Node-Name)
                Console.WriteLine($"Fehler beim interpretieren vom OPC-Node: {ex.Message}");
                return -2; // Rückgabe eines Fehlerwertes, wenn etwas schief geht
            }
        }


        private async Task<Dictionary<string, int>> ReadNodes(List<string> nodeNames)
        {
            try
            {
                // Ruft die Methode ReadNodesAsync aus dem Service auf, um die Werte der Nodes zu lesen
                var nodeResults = await _opcService.ReadNodesAsync(nodeNames);

                // Dictionary zur Rückgabe der Integer-Werte (0 = false, 1 = true)
                var intResults = new Dictionary<string, int>();

                foreach (var result in nodeResults)
                {
                    // Wandelt den booleschen Wert in einen Integer um und fügt ihn dem Dictionary hinzu
                    intResults[result.Key] = result.Value ? 1 : 0;
                }

                // Rückgabe der Ergebnisse als Dictionary
                return intResults;
            }
            catch (Exception ex)
            {
                // Fehlerprotokollierung
                Console.WriteLine($"Fehler beim Interpretieren der OPC-Nodes: {ex.Message}");

                // Rückgabe eines leeren Dictionaries oder einer speziellen Fehlermeldung
                return new Dictionary<string, int>
                {
                    { "Error", -2 }
                };
            }
        }







        // DEBUG ONLY
        //einzeneler Node
        [HttpPost("readSNode")]
        public async Task<IActionResult> ApiReadNode(string nodeName)
        {
            try
            {
                // Ruft die Methode ReadNode auf, die den Wert für den angegebenen Node-Namen liest und umwandelt.
                return Ok(new { Value = await ReadNode(nodeName) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }






        // DEBUG ONLY
        //mehrere Nodes
        [HttpPost("readSNodeList")]
        public async Task<IActionResult> ApiReadNodeList([FromBody] List<string> nodeNames)
        {

            try
            {
                // Ruft die Methode ReadNode auf, die den Wert für den angegebenen Node-Namen liest und umwandelt.
                return Ok(new { Value = await ReadNodes(nodeNames) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }




    }
}
