using Opc.Ua;
using Opc.Ua.Client;


namespace Maschinenampel.Server.Services
{
    public class OPC_Service : IHostedService
    {
        //OPC-Login Aktuell:
        //Anonym, ohne Sicherheit (none)

        private readonly IConfiguration _configuration;
        private readonly OPCServerConfiguration _opcServerConfiguration;
        private Session _session;
        private ApplicationConfiguration _appConfig;


        public OPC_Service(IConfiguration configuration)
        {
            _configuration = configuration;
            _opcServerConfiguration = new OPCServerConfiguration();
            configuration.GetSection("OPCServer").Bind(_opcServerConfiguration);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Debug-Log: Konfigurationseinstellungen ausgeben
                //Console.WriteLine($"Server URL: {_opcServerConfiguration.Url}");
                //Console.WriteLine($"Security Mode: {_opcServerConfiguration.SecurityMode}");

                // Anwendungskonfiguration erstellen
                _appConfig = new ApplicationConfiguration()
                {
                    ApplicationName = "OPCUAClienMaschinenamplen",
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier(),
                        AutoAcceptUntrustedCertificates = true, // Vorsicht bei Produktionssystemen
                        AddAppCertToTrustedStore = true
                    },
                    ClientConfiguration = new ClientConfiguration()
                };

                // Debug-Log: Überprüfen der Konfiguration
                Console.WriteLine("Validiere die OPC-Anwendungskonfiguration...");
                await _appConfig.Validate(ApplicationType.Client);

                // Sicherheitsmodus verarbeiten
                if (!Enum.TryParse<MessageSecurityMode>(
                        _opcServerConfiguration.SecurityMode,
                        out var securityMode))
                {
                    securityMode = MessageSecurityMode.None;
                    Console.WriteLine($"Warnung: Ungültiger SecurityMode, Standardwert 'None' verwendet.");
                }

                // Debug-Log: Sicherheitsmodus bestätigen
                Console.WriteLine($"Verwendeter Sicherheitsmodus: {securityMode}");

                // Endpoint aus Server-URL und SecurityMode auswählen
                var endpointDescription = CoreClientUtils.SelectEndpoint(
                    _opcServerConfiguration.Url,
                    securityMode != MessageSecurityMode.None);
                Console.WriteLine($"Endpoint URL: {endpointDescription.EndpointUrl}");

                var endpointConfiguration = EndpointConfiguration.Create(_appConfig);
                var endpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // Anonyme Authentifizierung (durch Übergabe von null)
                var userIdentity = new UserIdentity();  // Anonyme Authentifizierung
                Console.WriteLine("Verwende anonyme Authentifizierung.");

                // Benutzerdaten aus der Konfiguration laden. Noch nicht getestet!
                /*var userIdentity = new UserIdentity(
                    _opcServerConfiguration.Username,
                    _opcServerConfiguration.Password
                );
                Console.WriteLine("Verwende Benutzerdaten zur Authentifizierung.");*/



                // Session erstellen
                _session = await Session.Create(
                    _appConfig,
                    endpoint,
                    false,
                    "Session1",
                    60000,
                    userIdentity,
                    null);


                Console.WriteLine("Verbindung zum OPC UA Server erfolgreich hergestellt.");

                }
            catch (Exception ex)
            {
                // Detaillierte Fehlermeldung ausgeben
                Console.WriteLine($"Fehler beim Verbinden mit dem OPC UA Server: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // Weitere Informationen zum Fehler: Falls der Fehler von einer inneren Ausnahme kommt
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Innere Ausnahme: {ex.InnerException.Message}");
                    Console.WriteLine($"Innere Ausnahme StackTrace: {ex.InnerException.StackTrace}");
                }

                // Host beenden bei schwerwiegendem Fehler
                throw;
            }
        }




        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_session != null && _session.Connected)
            {
                await Task.Run(() => _session.Close());
                _session.Dispose();
                Console.WriteLine("Verbindung zum OPC UA Server getrennt.");
            }
        }





        //mehrere Nodes gleichzeitig auslesen
        public async Task<Dictionary<string, bool>> ReadNodesAsync(IEnumerable<string> nodes)
        {


            //Wenn Node "DEMO" enthält wird Demomodus aktiviert und Bitstatus zufällig gesetzt
            if (nodes.Any(node => node.Contains("DEMO")))
            {
                Console.WriteLine("--- DEMO AKTIV ---");
                return DemoBits(nodes);
            }





            // Dictionary für die Ergebnisse
            var results = new Dictionary<string, bool>();

            try
            {

                // Sicherstellen, dass die Session korrekt initialisiert ist
                if (_session == null)
                {
                    throw new InvalidOperationException("Die OPC UA Session ist nicht initialisiert.");
                }

                // Collection für die zu lesenden Nodes
                var nodesToRead = new ReadValueIdCollection();

                // Node-IDs aus der Eingabeliste erstellen
                //Node-ID aus namespace und Adresse im Stringformat zusammensetzten
                //https://stackoverflow.com/questions/57562971/what-is-the-significance-of-ns-2s-in-an-opc-node-path
                foreach (var node in nodes)
                {
                    string nodeId = "ns=" + _opcServerConfiguration.Opc_Namespace + ";s=" + node;
                    nodesToRead.Add(new ReadValueId
                    {
                        NodeId = new NodeId(nodeId),
                        AttributeId = Attributes.Value
                    });
                }

                // Leseoperation durchführen
                var readResult = _session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    nodesToRead,
                    out var dataValues,
                    out var diagnosticInfos
                );

                // Überprüfen, ob das Ergebnis gültig ist
                if (dataValues == null || dataValues.Count != nodesToRead.Count)
                {
                    throw new Exception("Fehler: Die Antwort enthält nicht alle angeforderten Werte.");
                }

                // Ergebnisse durchlaufen und boolean-Werte extrahieren
                for (int i = 0; i < dataValues.Count; i++)
                {
                    var dataValue = dataValues[i];
                    var nodeId = nodesToRead[i].NodeId.ToString();

                    // StatusCode prüfen
                    if (dataValue.StatusCode == Opc.Ua.StatusCodes.Good)
                    {
                        // Wert prüfen und hinzufügen
                        if (dataValue.Value is bool booleanValue)
                        {
                            results[nodeId] = booleanValue;
                                                        
                        }
                        else
                        {
                            throw new InvalidOperationException($"Der Wert von Node {nodeId} ist kein boolescher Typ.");
                        }
                    }
                    else
                    {
                        throw new Exception($"Fehler beim Lesen von Node {nodeId}: {dataValue.StatusCode}");
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                // Fehlerprotokollierung
                Console.WriteLine($"Fehler beim Lesen der Nodes: {ex.Message}");
                throw;
            }
        }





        //DEMO Only
        private static readonly Random random = new Random();
        public static Dictionary<string, bool> DemoBits(IEnumerable<string> nodes)
        {
            // Neues Dictionary erstellen
            var res = new Dictionary<string, bool>();

            // Durch die nodes iterieren und zufällige bool-Werte zuweisen
            foreach (var node in nodes)
            {
                res[node] = random.Next(2) == 0; // 0 -> false, 1 -> true
            }

            return res;
        }








    }



    public class OPCServerConfiguration
    {
        //Appsettings-Variablen vorbereiten
        public string Url { get; set; }
        public string Username { get; set; } //derzeit nicht verwendet
        public string Password { get; set; } //derzeit nicht verwendet
        public string SecurityMode { get; set; } = "None"; // Standard: Keine Sicherheit
        public string Opc_Namespace { get; set; } = "2"; // Standard: NS=2
    }
}
