//https://youtu.be/9ZD7cKIaxdM Video Tutorial

using Microsoft.AspNetCore.Mvc; // Namespace für ASP.NET Core MVC
using Microsoft.Data.SqlClient; // Namespace für SQL Server-Datenbankzugriffe
using System.Data; // Namespace für DataTable und andere Datenstrukturen

namespace Maschinenampel.Server.Controllers
{
    // Definiert die Route für diesen Controller
    [Route("api/OPCController")]
    [ApiController] // Kennzeichnet die Klasse als API-Controller
    [RequireHttps] // Erzwingt die Verwendung von HTTPS für alle Anfragen

    public class OPC_Controller : ControllerBase
    {
    }
}