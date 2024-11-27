﻿using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;


namespace Maschinenampel.Server.Controllers
{
    [ApiController]
    [Route("api/imgUpload")]
    public class UploadController : ControllerBase
    {
        private readonly string _uploadPath = "./wwwroot/images";

        public UploadController()
        {
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Kein gültiges Bild hochgeladen.");
            }

            try
            {
                // **Dateinamen mit Zeitstempel erstellen um Duplikate zu verhindern**
                // Aktuelle Uhrzeit und Datum als eindeutiger Zeitstempel im Format yyyyMMddHHmmss
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileExtension = Path.GetExtension(file.FileName);// Extrahiert die Dateiendung, z. B. ".jpg"
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);// Extrahiert den Dateinamen ohne die Endung
                var newFileName = $"{fileNameWithoutExt}_{timeStamp}{fileExtension}";// Erstellt den neuen Dateinamen mit Zeitstempel
                var filePath = Path.Combine(_uploadPath, newFileName);// Kombiniert den Speicherpfad mit dem neuen Dateinamen

                // **Datei speichern**
                // Öffnet einen Stream zum Erstellen der Datei am definierten Speicherort
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    // Kopiert die hochgeladene Datei in den Stream
                    await file.CopyToAsync(stream);
                }

                // **Bildgröße auslesen mit ImageSharp**
                using var image = await Image.LoadAsync(filePath); // Bild laden
                int width = image.Width;
                int height = image.Height;
                float aspectRatio = (float)width / height; // Seitenverhältnis berechnen

                // **Antwort mit zusätzlichen Informationen zurückgeben**
                return Ok(new
                {
                    URL = filePath.Substring(9), // Entfernt ./wwwroot aus der URL
                    aspectRatio = Math.Round(aspectRatio, 4)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}