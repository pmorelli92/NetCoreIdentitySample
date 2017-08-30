using Microsoft.AspNetCore.Mvc;

namespace NetCore.Identity.Sample.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => Json("sarasa");
    }
}