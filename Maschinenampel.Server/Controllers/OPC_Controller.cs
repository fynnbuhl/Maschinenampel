//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC

namespace Maschinenampel.Server.Controllers
{
    
    [Route("api/OPCController")] // Definiert die Route für diesen Controller
    [ApiController]
    [RequireHttps]
    public class OPC_Controller : ControllerBase
    {

        [HttpPost]
        [Route("getBITs")]
        public IActionResult getBits([FromBody] string[][] OPC_NameArray)
        {
            //TODO: NameArray an OPC-Controller geben und BIT Array zurück erhalten
            string[][] OPC_BITArray = new string[][]
            {
                new string[] { "1", "0"},
                new string[] { "0", "1", "0" }
            };

            // Gib das empfangene Array zurück
            return Ok(OPC_NameArray);
        }
    }
}
