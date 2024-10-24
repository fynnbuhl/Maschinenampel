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
                return Ok(new { Message = "Dashboard added successfully." }); // Erfolgreiche Rückmeldung
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }

        // Asynchrone Methode zum Entfernen eines Dashboards
        [HttpPost] // HTTP POST-Anforderung
        [Route("removeDashboard")] // Spezifische Route für diese Methode
        public async Task<IActionResult> RemoveDashboardAsync([FromForm] int ID)
        {
            // Überprüfen, ob die ID gültig ist
            if (ID <= 0)
            {
                return BadRequest(new { Message = "Invalid ID" }); // Gibt einen 400 Fehler zurück, wenn die ID ungültig ist
            }

            string query = "DELETE FROM DashboardDB WHERE ID = @ID"; // SQL-Abfrage zum Entfernen eines Dashboards

            try
            {
                // Parameter für die SQL-Abfrage erstellen
                var parameters = new[] { new SqlParameter("@ID", ID) };

                // Führt die SQL-Abfrage aus und gibt die Anzahl der betroffenen Zeilen zurück
                int rowsAffected = await ExecuteNonQueryAsync(query, parameters);

                if (rowsAffected > 0) // Überprüft, ob eine Zeile entfernt wurde
                {
                    return Ok(new { Message = "Dashboard removed successfully." }); // Erfolgreiche Rückmeldung
                }
                else
                {
                    return NotFound(new { Message = "Dashboard not found." }); // Gibt einen 404 Fehler zurück, wenn kein Dashboard gefunden wurde
                }
            }
            catch (Exception ex) // Fehlerbehandlung
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message }); // Gibt einen 500 Fehler zurück
            }
        }

        // Hilfsmethode zur Ausführung eines SQL-Select-Befehls
        private async Task<DataTable> ExecuteQueryAsync(string query)
        {
            DataTable dataTable = new DataTable(); // Erstellt ein neues DataTable für die Ergebnisse
            string sqlDatasource = _configuration.GetConnectionString("DBConnect"); // Datenbank-Verbindungszeichenfolge aus appsettings.json 

            // Öffnet eine Verbindung zur Datenbank
            using (SqlConnection connection = new SqlConnection(sqlDatasource))
            {
                await connection.OpenAsync(); // Asynchrone Öffnung der Verbindung
                using (SqlCommand command = new SqlCommand(query, connection)) // SQL-Befehl erstellen
                {
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
