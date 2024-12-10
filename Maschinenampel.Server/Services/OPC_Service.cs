using Opc.Ua;
using Opc.Ua.Client;


namespace Maschinenampel.Server.Services
{
    public class OPC_Service : IHostedService
    {
        //TODO: OPC-Login
        //Aktuell: Anonym, ohne Sicherheit (none)

        private readonly IConfiguration _configuration;
        private readonly OPCServerConfiguration _opcServerConfiguration;
        private Session _session;
        private ApplicationConfiguration _appConfig;

        private bool _isInitialized = false;

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
                Console.WriteLine("Validiere die Anwendungskonfiguration...");
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

                // Benutzerdaten aus der Konfiguration laden
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

                _isInitialized = true;

                Console.WriteLine("Verbindung zum OPC UA Server erfolgreich hergestellt.");

                //BSP Node auslesen. DEBUG ONLY
                //await ReadNodeAsync("Beispiele für Datentyp.16 Bit-Gerät.B-Register.Boolean1");
                
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
                _isInitialized = false;
                Console.WriteLine("Verbindung zum OPC UA Server getrennt.");
            }
        }







        public async Task<bool> ReadNodeAsync(string node)
        {
            //Node-ID aus namespace und Adresse im Stringformat zusammensetzten
            //https://stackoverflow.com/questions/57562971/what-is-the-significance-of-ns-2s-in-an-opc-node-path
            string nodeId = "ns=" + _opcServerConfiguration.Opc_Namespace + ";s=" + node;

            try
            {
                // Sicherstellen, dass die Session korrekt initialisiert ist
                if (_session == null)
                {
                    throw new InvalidOperationException("Die OPC UA Session ist nicht initialisiert.");
                }

                // NodeId erstellen
                var nodeToRead = new ReadValueId
                {
                    NodeId = new NodeId(nodeId),
                    AttributeId = Attributes.Value
                };

                // Leseoperation durchführen
                var readResult = _session.Read(
                    null,
                    0,
                    TimestampsToReturn.Both,
                    new ReadValueIdCollection { nodeToRead },
                    out var dataValues,
                    out var diagnosticInfos
                );

                // Überprüfen, ob das Ergebnis gültig ist
                if (dataValues == null || dataValues.Count == 0)
                {
                    throw new Exception("Fehler: Die Antwort enthält keine Werte.");
                }

                // Überprüfung des Statuscodes
                if (dataValues[0].StatusCode != Opc.Ua.StatusCodes.Good)
                {
                    throw new Exception($"Fehler beim Lesen des Nodes: {dataValues[0]?.StatusCode}");
                }



                // Durchlaufen der dataValues und Ausgabe der Details. DEBUG ONLY
                /*foreach (var dataValue in dataValues)
                {
                    Console.WriteLine($"NodeId {nodeId}: {dataValue.Value}");
                }*/


                // Erfolgreiche Rückgabe des Werts als bool
                if (dataValues[0].Value is bool booleanValue)
                {
                    return booleanValue;
                }
                else
                {
                    throw new InvalidOperationException("Der Wert ist kein boolescher Typ.");
                }

            }
            catch (Exception ex)
            {
                // Fehlerprotokollierung
                Console.WriteLine($"Fehler beim Lesen des Nodes {nodeId}: {ex.Message}");
                throw;
            }
        }






    }



    public class OPCServerConfiguration
    {
        //Appsettings-Variablen vorbereiten
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SecurityMode { get; set; } = "None"; // Standard: Keine Sicherheit
        public string Opc_Namespace { get; set; } = "2";
    }
}
