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
    }
}
