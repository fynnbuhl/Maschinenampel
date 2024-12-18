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

        private readonly OPC_Service _opcService; // Service für das Abrufen von OPC-Daten
        private readonly IConfiguration _configuration;
        private readonly OPCServerConfiguration _opcServerConfiguration;

        // Konstruktor der Klasse, der den OPC_Service für den Zugriff auf die OPC-Daten injiziert
        public OPC_Controller(OPC_Service opcService, IConfiguration configuration)
        {
            _opcService = opcService;
            _configuration = configuration;
            _opcServerConfiguration = new OPCServerConfiguration();
            configuration.GetSection("OPCServer").Bind(_opcServerConfiguration);
        }



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


                    // Verzögerung von x Sekunden, bevor der Vorgang wiederholt wird
                    // siehe appsettings.json
                    //Console.WriteLine("UpdateIntervall: " + _opcServerConfiguration.UpdateIntervall_ms);
                    await Task.Delay(_opcServerConfiguration.UpdateIntervall_ms);
                }
            }
            finally
            {
                await receiveTask; // Stelle sicher, dass der Empfangs-Task beendet wird
                OPC_AddrArray = new string[0][];
            }


        }





        // Eine Liste zur Speicherung der Node-Namen (Adressen), die später gelesen werden sollen
        private List<string> nodeNames = new List<string> { };

        // Diese Methode wird verwendet, um die aktuellen Bitzustände (true/false als 1/0) der übergebenen Adressen abzurufen und in einem 2D-Array (int[][]) zu speichern.
        private async Task<int[][]> UpdateBits(string[][] OPC_Addr, int[][] OPC_Bits)
        {
            // Äußere Schleife: Iteration durch die Zeilen des 2D-Arrays OPC_Addr (über alle Amplen)
            for (int i = 0; i < OPC_Addr.Length; i++)
            {
                // Innere Schleife: Iteration durch jede Spalte (Adresse) innerhalb der aktuellen Zeile (einer Ampel)
                for (int j = 0; j < OPC_Addr[i].Length; j++)
                {
                    // DEBUG-Log: Gibt die aktuelle Position und Adresse in der Konsole aus
                    // Console.WriteLine($"POS: {i},{j}, Addr: {OPC_Addr[i][j]}");

                    // Fügt die aktuelle Adresse zur Liste `nodeNames` hinzu, um die gesammmte Ampel zusammenzusetzten
                    nodeNames.Add(OPC_Addr[i][j]);
                }


                try
                {
                    // Ruft pro Ampel die Methode `ReadNodesAsync` auf, die alle Adressen in `nodeNames` gleichzeitig ausliest.
                    // Ergebnis ist ein Dictionary<string, bool>, das den Node-Namen mit seinem aktuellen Wert (true/false) verknüpft.
                    var nodeResults = await _opcService.ReadNodesAsync(nodeNames);
                                    
                    // Dictionary zur Zwischenspeicherung der Ergebnisse als Integer-Werte
                    // (0 für false, 1 für true).
                    var intResults = new Dictionary<string, int>();

                    // Durchlaufen der Ergebnisse von `ReadNodesAsync`
                    foreach (var result in nodeResults)
                    {
                        // Konvertiert den booleschen Wert (`result.Value`) in einen Integer-Wert (0 oder 1)
                        // und speichert diesen im `intResults`-Dictionary, wobei die Node-ID als Schlüssel verwendet wird.
                        intResults[result.Key] = result.Value ? 1 : 0;

                        // Weist die konvertierten Integer-Werte der aktuellen Zeile `OPC_Bits[i]` zu.
                        // `intResults.Values.ToArray()` konvertiert die Werte des Dictionaries in ein Array.
                        OPC_Bits[i] = intResults.Values.ToArray();
                    }

                    // Leert die Liste `nodeNames`, damit sie für die nächste Zeile (nächsten Durchlauf) neu befüllt werden kann.
                    nodeNames.Clear();

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fehler beim Interpretieren der Nodes: {ex.Message}");
                    OPC_Bits[i] = [-2];
                }
            }

            // Gibt das aktualisierte 2D-Array zurück, das die konvertierten Integer-Bitwerte für alle Adressen enthält.
            return OPC_Bits;
        }






        // DEBUG ONLY
        //mehrere Nodes
        [HttpPost("readSNodeList")]
        public async Task<IActionResult> ApiReadNodeList([FromBody] List<string> nodeNames) //BSP: ["Beispiele für Datentyp.16 Bit-Gerät.B-Register.Boolean1"]
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
                    Console.WriteLine(intResults[result.Key]);
                }

                return Ok(new { Values = intResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }




    }


    public class OPCServerConfiguration
    {
        //Appsettings-Variablen vorbereiten
        public int UpdateIntervall_ms { get; set; }
    }
}
