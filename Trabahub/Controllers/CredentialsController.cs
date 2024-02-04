using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabahub.Data;

namespace Trabahub.Controllers
{
    public class CredentialsController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        public CredentialsController(ApplicationDbContext context)
        {
            _context = context;

        }
        public IActionResult Login()
        {
            var credentials = _context.Credentials.ToList();
            return View(credentials);
        }

        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(string user, string password)
        {
            var credentials = _context.Credentials.ToList();
            return View(credentials);
        }

        [HttpPost]
        [ActionName("Register")]
        public IActionResult Register()
        {
            return View();
        }

        //EMAIL PASSWORD
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
