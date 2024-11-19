//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC
using Microsoft.Data.SqlClient; // Namespace für SQL Server-Datenbankzugriffe
using System.Data; // Namespace für DataTable und andere Datenstrukturen

namespace Maschinenampel.Server.Controllers
{
    // Definiert die Route für diesen Controller
    [Route("api/DBController")]
    [ApiController] // Kennzeichnet die Klasse als API-Controller
    [RequireHttps] // Erzwingt die Verwendung von HTTPS für alle Anfragen
    public class DB_Controller : ControllerBase
    {
        private readonly IConfiguration _configuration; // Konfiguration für den Zugriff auf Einstellungen

        // Konstruktor, der die IConfiguration bereitstellt
        public DB_Controller(IConfiguration configuration)
        {
            _configuration = configuration; // Initialisierung der Konfiguration
        }






        // Asynchrone Methode zur Abfrage von Dashboards
        [HttpGet] // HTTP GET-Anforderung
        [Route("getDashboards")] // Spezifische Route für diese Methode
        public async Task<IActionResult> GetDashboardsAsync()
        {
            string query = "SELECT * FROM DashboardDB"; // SQL-Abfrage zum Abrufen aller Dashboards
            try
            {
                // Führt die Abfrage aus und lädt die Ergebnisse in ein DataTable
                DataTable tableDashboards = await ExecuteQueryAsync(query);
                return Ok(tableDashboards); // Gibt die Ergebnisse als HTTP 200 OK zurück
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler mit der Fehlermeldung zurück
            }
        }





        // Asynchrone Methode zur Abfrage von Amplen eines Dashboards
        [HttpGet] // HTTP GET-Anforderung
        [Route("getAmpeln")] // Spezifische Route für diese Methode
        public async Task<IActionResult> GetAmpelnAsync([FromQuery] int selected_ID)
        {

            string query = "SELECT * FROM AmpelDB WHERE DASHBOARD_ID = @selected_ID"; // SQL-Abfrage zum Abrufen aller Ampeln der gesetzten ID

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[] { new SqlParameter("@selected_ID", selected_ID) };

                // Führt die Abfrage aus und lädt die Ergebnisse in ein DataTable
                DataTable tableAmplen = await ExecuteQueryAsync(query, parameters);
                return Ok(tableAmplen); // Gibt die Ergebnisse als HTTP 200 OK zurück
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler mit der Fehlermeldung zurück
            }
        }









        // Asynchrone Methode zum Hinzufügen eines Dashboards
        [HttpPost] // HTTP POST-Anforderung
        [Route("addDashboard")] // Spezifische Route für diese Methode
        public async Task<IActionResult> AddDashboardAsync([FromForm] string Name, [FromForm] string IMG_PATH)
        {
            // Überprüfen, ob die Eingabewerte leer sind
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(IMG_PATH))
            {
                return BadRequest(new { Message = "Name and IMG_PATH cannot be empty" }); // Gibt einen 400 Fehler zurück, wenn Eingaben fehlen
            }

            string query = "INSERT INTO DashboardDB (Name, IMG_PATH) VALUES(@Name, @IMG_PATH)"; // SQL-Abfrage zum Hinzufügen eines Dashboards

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[]
                {
                    new SqlParameter("@Name", Name),
                    new SqlParameter("@IMG_PATH", IMG_PATH)
                };

                // Führt die SQL-Abfrage aus, die keine Rückgabewerte hat
                await ExecuteNonQueryAsync(query, parameters);
                return Ok(new { Message = "Dashboard hinzugefügt." }); // Erfolgreiche Rückmeldung
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }



        // Asynchrone Methode zum Hinzufügen einer Ampel
        [HttpPost] // HTTP POST-Anforderung
        [Route("addAmpel")] // Spezifische Route für diese Methode
        public async Task<IActionResult> AddAmpelAsync([FromQuery] int Dashboard_ID, [FromForm] int POS_X, [FromForm] int POS_Y, [FromForm] int SIZE, [FromForm] int ColorCount, [FromForm] string COLORS, [FromForm] string OPC_BIT)
        {

            string query = "INSERT INTO AmpelDB (DASHBOARD_ID, POS_X, POS_Y, SIZE, ColorCount, COLORS, OPC_BIT) VALUES(@DASHBOARD_ID, @POS_X, @POS_Y, @SIZE, @ColorCount, @COLORS, @OPC_BIT)"; // SQL-Abfrage zum Hinzufügen eines Dashboards

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[]
                {
                    new SqlParameter("@Dashboard_ID", Dashboard_ID),
                    new SqlParameter("@POS_X", POS_X),
                    new SqlParameter("@POS_Y", POS_Y),
                    new SqlParameter("@SIZE", SIZE),
                    new SqlParameter("@ColorCount", ColorCount),
                    new SqlParameter("@COLORS", COLORS),
                    new SqlParameter("@OPC_BIT", OPC_BIT)
                };

                 // Führt die SQL-Abfrage aus, die keine Rückgabewerte hat
                await ExecuteNonQueryAsync(query, parameters);
                return Ok(new { Message = "Ampel hinzugefügt." }); // Erfolgreiche Rückmeldung

            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message}); // Gibt einen 500 Fehler zurück
            }
        }








        // Asynchrone Methode zum Aktualisieren des Dashboard-Namens
        [HttpPost]
        [Route("updateDashboardName")] // Spezifische Route für diese Methode
        public async Task<IActionResult> UpdateDashboardNameAsync([FromQuery] int ID, [FromForm] string NewName)
        {

            string query = "UPDATE DashboardDB SET Name = @NewName WHERE ID = @ID"; // SQL-Abfrage zum Aktualisieren des Dashboard-Namens

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[]
                {
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@NewName", NewName)
                };

                // Führt die SQL-Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);

                if (rowsAffected > 0)
                {
                    return Ok(new { Message = "Dashboard Name aktualisiert." }); // Erfolgreiche Rückmeldung
                }
                else
                {
                    return NotFound(new { Message = "Dashboard nicht gefunden, ID unbekannt." }); // Gibt einen 404 Fehler zurück, wenn kein Dashboard gefunden wurde
                }
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }



        // Asynchrone Methode zum aktualisieren einer Ampel
        [HttpPost] // HTTP POST-Anforderung
        [Route("updateAmpel")] // Spezifische Route für diese Methode
        public async Task<IActionResult> updateAmpelAsync([FromQuery] int ID, [FromForm] int Dashboard_ID, [FromForm] int POS_X, [FromForm] int POS_Y, [FromForm] int SIZE, [FromForm] int ColorCount, [FromForm] string COLORS, [FromForm] string OPC_BIT)
        {

            // SQL-Abfrage zum Aktualisieren der Ampel-Einträge
            string query = @"UPDATE AmpelDB 
                     SET POS_X = @POS_X, 
                         POS_Y = @POS_Y, 
                         SIZE = @SIZE, 
                         ColorCount = @ColorCount, 
                         COLORS = @COLORS, 
                         OPC_BIT = @OPC_BIT 
                     WHERE ID = @ID";

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[]
                {
                    new SqlParameter("@ID", ID),
                    new SqlParameter("@Dashboard_ID", Dashboard_ID),
                    new SqlParameter("@POS_X", POS_X),
                    new SqlParameter("@POS_Y", POS_Y),
                    new SqlParameter("@SIZE", SIZE),
                    new SqlParameter("@ColorCount", ColorCount),
                    new SqlParameter("@COLORS", COLORS),
                    new SqlParameter("@OPC_BIT", OPC_BIT)
                };

                // Führt die SQL-Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);
                if (rowsAffected > 0)
                {
                    return Ok(new { Message = "Ampel aktualisiert." }); // Erfolgreiche Rückmeldung
                }
                else
                {
                    return NotFound(new { Message = "Ampel nicht gefunden, ID unbekannt." }); // Gibt einen 404 Fehler zurück, wenn keine Ampel gefunden wurde
                }

            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }








        // Asynchrone Methode zum Entfernen eines Dashboards
        [HttpPost] // HTTP POST-Anforderung
        [Route("deleteDashboard")] // Spezifische Route für diese Methode
        public async Task<IActionResult> DeleteDashboardAsync([FromQuery] int ID)
        {
            string query = "DELETE FROM DashboardDB WHERE ID = @ID"; // SQL-Abfrage zum Entfernen eines Dashboards

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[] { new SqlParameter("@ID", ID) };

                // Löscht alle verknüpften Zeilen in AmpelDB
                await DeleteAllAmpelnAsync(ID);

                // Führt die SQL-Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);

                if (rowsAffected > 0) // Überprüft, ob eine Zeile entfernt wurde
                {
                    return Ok(new { Message = "Dashboard gelöscht." }); // Erfolgreiche Rückmeldung
                }
                else
                {
                    return NotFound(new { Message = "Dashboard nicht gefunden, ID unbekannt." }); // Gibt einen 404 Fehler zurück, wenn kein Dashboard gefunden wurde
                }
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }



        // Asynchrone Methode zum Entfernen aller Ampeln eines Dashboards
        [HttpPost] // HTTP POST-Anforderung
        [Route("deleteAllAmpeln")] // Spezifische Route für diese Methode
        public async Task<IActionResult> DeleteAllAmpelnAsync(int selected_ID)
        {
            // SQL-Abfrage, die alle Zeilen mit der entsprechenden DASHBOARD_ID löscht
            string query = "DELETE FROM AmpelDB WHERE DASHBOARD_ID = @selected_ID";

            try
            {
                // SQL-Parameter erstellen und mit der übergebenen ID befüllen
                var parameters = new[] { new SqlParameter("@selected_ID", selected_ID)};

                // Führt die Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);
                return Ok(new { Message = "Ampeln gelöscht." }); // Erfolgreiche Rückmeldung
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                // Gibt bei einem Fehler eine detaillierte Fehlermeldung zurück
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }



        // Asynchrone Methode zum Entfernen einer Ampel
        [HttpPost] // HTTP POST-Anforderung
        [Route("deleteAmpel")] // Spezifische Route für diese Methode
        public async Task<IActionResult> DeleteAmpelAsync([FromQuery] int ID)
        {
            // SQL-Abfrage, die alle Zeilen mit der entsprechenden DASHBOARD_ID löscht
            string query = "DELETE FROM AmpelDB WHERE ID = @ID";

            try
            {
                // SQL-Parameter erstellen und mit der übergebenen ID befüllen
                var parameters = new[] { new SqlParameter("@ID", ID) };

                // Führt die Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);

                if (rowsAffected > 0)
                {
                    return Ok(new { Message = "Ampel gelöscht." }); // Erfolgreiche Rückmeldung
                }
                else
                {
                    return NotFound(new { Message = "Ampel nicht gefunden, ID unbekannt." }); // Keine Zeilen gefunden
                }
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                // Gibt bei einem Fehler eine detaillierte Fehlermeldung zurück
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message });
            }
        }










        // Hilfsmethode zur Ausführung eines SQL-Select-Befehls
        private async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable(); // Erstellt ein neues DataTable für die Ergebnisse
            string sqlDatasource = _configuration.GetConnectionString("DBConnect"); // Datenbank-Verbindungszeichenfolge aus appsettings.json 

            // Öffnet eine Verbindung zur Datenbank
            using (SqlConnection connection = new SqlConnection(sqlDatasource))
            {
                await connection.OpenAsync(); // Asynchrone Öffnung der Verbindung

                using (SqlCommand command = new SqlCommand(query, connection)) // SQL-Befehl erstellen
                {
                    // Binde die Parameter an den SQL-Befehl, falls vorhanden
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (SqlDataReader reader = await command.ExecuteReaderAsync()) // Asynchrone Ausführung des Lesers
                    {
                        dataTable.Load(reader); // Lädt die Ergebnisse in das DataTable
                    }
                }
            }
            return dataTable; // Gibt das DataTable mit den Ergebnissen zurück
        }


        // Hilfsmethode zur Ausführung eines SQL-Befehls (z.B. INSERT, UPDATE, DELETE)
        private async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters)
        {
            string sqlDatasource = _configuration.GetConnectionString("DBConnect"); // Datenbank-Verbindungszeichenfolge aus appsettings.json 

            // Öffnet eine Verbindung zur Datenbank
            using (SqlConnection connection = new SqlConnection(sqlDatasource))
            {
                await connection.OpenAsync(); // Asynchrone Öffnung der Verbindung
                using (SqlCommand command = new SqlCommand(query, connection)) // SQL-Befehl erstellen
                {
                    command.Parameters.AddRange(parameters); // Fügt die Parameter zum Befehl hinzu
                    return await command.ExecuteNonQueryAsync(); // Führt den Befehl aus und gibt die Anzahl der betroffenen Zeilen zurück
                }
            }
        }
    }
}
