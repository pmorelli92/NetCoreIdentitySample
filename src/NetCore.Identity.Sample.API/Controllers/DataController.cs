using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class DataController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "LowSec, HighSec")]
        public IActionResult LowSecurityLevel()
        {
            return Json("Low Security overriden");
        }

        [HttpGet]
        [Authorize(Roles = "HighSec")]
        public IActionResult HighSecurityLevel()
        {
            return Json("High Security overriden");
        }

        [HttpGet]
        [Authorize(Roles = "LowSec, HighSec")]
        public IActionResult Claims()
        {
            return Json(User.Claims.Select(e => new
            {
                key = e.Type,
                value = e.Value
            }));
        }
    }
}