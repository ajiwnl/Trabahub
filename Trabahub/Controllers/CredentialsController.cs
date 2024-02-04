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

        public IActionResult Index()
        {
            var credentials = _context.Credentials.ToList();
            return View(credentials);
        }


        [HttpPost]
        [ActionName("Login")]
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

        //EMAIL PASSWORD
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}
