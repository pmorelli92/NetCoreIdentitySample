using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class DataController : Controller
    {
        [HttpGet]
        [Authorize(Policy = "ApiUser")]
        public IActionResult ConfidentialData()
        {
            var asd = User.Claims;


            return Json("ConfidentialData");
        }
    }
}