//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC
using System.Data;

namespace Maschinenampel.Server.Controllers
{
    
    [Route("api/OPCController")] // Definiert die Route für diesen Controller
    [ApiController]
    [RequireHttps]
    public class OPC_Controller : ControllerBase
    {

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

            try
            {
                // Beispiel für dynamische Erstellung eines BIT Arrays.
                // Hier könnte eine asynchrone Methode zum Abrufen der BIT-Werte von einem OPC-Server integriert werden.
                int[][] OPC_BITArray = await GetOPCBitsFromControllerAsync(model.OPC_BIT_Addr);

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





    }
}
