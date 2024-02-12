using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class ListingController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
