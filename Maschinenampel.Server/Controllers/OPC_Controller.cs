//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC
using Microsoft.Data.SqlClient; // Namespace für SQL Server-Datenbankzugriffe
using System.Data; // Namespace für DataTable und andere Datenstrukturen

namespace Maschinenampel.Server.Controllers
{
    
    [Route("api/OPCController")] // Definiert die Route für diesen Controller
    [ApiController]
    [RequireHttps]
    public class OPC_Controller : ControllerBase
    {
        // Eine einfache Testmethode, die einen Statuscode und eine Nachricht zurückgibt
        [HttpGet]
        [Route("test")] // Definiert die Route für diese Methode: api/OPCController/test
        public IActionResult TestMethod()
        {
            // Gibt einen OK-Status (200) und eine einfache Nachricht zurück
            return Ok(new { message = "OPC Controller ist funktionsfähig!" });
        }











    }
}
