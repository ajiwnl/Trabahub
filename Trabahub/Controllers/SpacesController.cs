using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class SpacesController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Spaces";
            return View();
        }

        public IActionResult Detail()
        {
            ViewData["ActivePage"] = "Details";
            return View();
        }

        public IActionResult Booking()
        {
            ViewData["ActivePage"] = "Booking";
            return View();
        }
    }
}
