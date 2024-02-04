using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabahub.Data;
using Trabahub.Models;

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

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(string emaillog, string passwordlog)
        {
			var existingEmail = _context.Credentials.FirstOrDefault(x => x.Email == emaillog);
			if(existingEmail == null)
            {
                TempData["UserNull"] = "Email Not Found";
                return View("Login");
            }

            if(existingEmail.Password != passwordlog)
            {
				TempData["PasswordIncorrect"] = "Incorrect Password";
                return View("Login");
			}
            string loggedUser = existingEmail.Username.ToString();

            TempData["Username"] = loggedUser;
            TempData["Status"] = "false";

            TempData["SuccessMessage"] = "Welcome To Trabahub!  " + loggedUser;
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ActionName("Register")]
        public IActionResult Register(Credentials addCredentialEntry)
        {
            var existingEmail = _context.Credentials.FirstOrDefault(x => x.Email == addCredentialEntry.Email);
            var existingUser =  _context.Credentials.FirstOrDefault(x => x.Username == addCredentialEntry.Username);

            if (existingEmail != null)
            {
                TempData["EmailExists"] = "Email Already Exists";
                return View("Register");
            }

            if (existingUser != null)
            {
                TempData["UserExists"] = "Username Already Exists";
                return View("Register");
            }

            SaveEntry(addCredentialEntry);
            TempData["SuccessMessage"] = "Successfully Registered! Try Logging In";
            return View("Login");
        }

        //EMAIL PASSWORD
        public IActionResult ForgotPassword()
        {
            return View();
        }

        public void SaveEntry(Credentials addCredentialEntry)
        {
            var credentials = new Credentials()
            {
                Email = addCredentialEntry.Email,
                Username = addCredentialEntry.Username,
                Password = addCredentialEntry.Password
            };

            _context.Credentials.Add(credentials);
            _context.SaveChanges();
        }
    }
}
