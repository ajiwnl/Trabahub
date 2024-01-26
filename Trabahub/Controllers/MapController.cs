using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class MapController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
