using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class SpacesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
