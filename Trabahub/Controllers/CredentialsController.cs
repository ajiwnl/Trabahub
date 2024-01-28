using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class CredentialsController : Controller
    {
        //GET
        public IActionResult Login()
        {
            return View();
        }
        
        //POST USER
        public IActionResult Register()
        {
            return View();
        }

        //POST OWNERS
        public IActionResult RegisterOwners()
        {
            return View();
        }
    }
}
