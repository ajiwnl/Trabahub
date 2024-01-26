using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class Community : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
