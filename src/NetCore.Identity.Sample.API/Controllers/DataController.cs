using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class DataController : Controller
    {
        [HttpGet]
        [Authorize(Policy = "SecurityLevel1")]
        public IActionResult ConfidentialData()
        {
            return Json("Secure date accessed");
        }

        [HttpGet]
        public IActionResult Claims()
        {
            return Json(User.Claims);
        }
    }
}