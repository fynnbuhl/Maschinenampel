using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Maschinenampel.Server.Controllers
{
    [ApiController]
    [Route("api/imgUpload")] // Legt die Basisroute für die Endpunkte in diesem Controller fest
    public class UploadController : ControllerBase
    {
        // Der Pfad, in dem die hochgeladenen Dateien gespeichert werden
        private readonly string _uploadPath = "./wwwroot/images";

        // Konstruktor des Controllers
        public UploadController()
        {
            // Überprüft, ob das Verzeichnis existiert. Falls nicht, wird es erstellt.
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        // HTTP POST-Endpunkt für den Datei-Upload
        [HttpPost]

        [Route("upload")] // Legt die spezifische Route für diesen Endpunkt fest: api/imgUpload/upload
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // Überprüft, ob eine Datei hochgeladen wurde und ob sie Daten enthält
            if (file == null || file.Length == 0)
            {
                return BadRequest("Kein gültiges Bild hochgeladen."); // Fehlerantwort mit HTTP-Status 400
            }

            try
            {
                // **Dateinamen mit Zeitstempel erstellen um Duplikate zu verhindern**
                // Aktuelle Uhrzeit und Datum als eindeutiger Zeitstempel im Format yyyyMMddHHmmss
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");

                // Extrahiert die Dateiendung, z. B. ".jpg"
                var fileExtension = Path.GetExtension(file.FileName);

                // Extrahiert den Dateinamen ohne die Endung
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);

                // Erstellt den neuen Dateinamen mit Zeitstempel
                var newFileName = $"{fileNameWithoutExt}_{timeStamp}{fileExtension}";

                // Kombiniert den Speicherpfad mit dem neuen Dateinamen
                var filePath = Path.Combine(_uploadPath, newFileName);

                // **Datei speichern**
                // Öffnet einen Stream zum Erstellen der Datei am definierten Speicherort
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    // Kopiert die hochgeladene Datei in den Stream
                    await file.CopyToAsync(stream);
                }

                // **Antwort mit dem neuen Dateinamen zurückgeben**
                // Gibt den Dateipfad als URL zurück (JSON-Objekt mit der Eigenschaft "URL")
                return Ok(new { URL = filePath.Substring(9) }); //entferne ./wwwroot aus der URL
            }
            catch (Exception ex) // Fängt alle möglichen Fehler während des Upload-Prozesses ab
            {
                // Gibt eine Fehlerantwort zurück, falls eine Ausnahme auftritt
                return BadRequest(ex.Message); // HTTP-Status 400 mit der Fehlermeldung
            }
        }
    }
}
