using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class ContactsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Contact Us";
            return View();
        }
    }
}
