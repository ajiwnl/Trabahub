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

        public IActionResult RedirectPage() { 
        
            return View();
        }

        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(string emaillog, string passwordlog)
        {
			var existingEmail = _context.Credentials.FirstOrDefault(x => x.Email == emaillog);
            var forgotMessage = TempData["ForgotMessage"];

            if (forgotMessage != null)
            {
                TempData.Remove("ForgotMessage");

                ViewBag.ForgotMessage = forgotMessage;

                // Check if SweetAlert flag is set
                if (TempData.Peek("ShowSweetAlert") is bool showSweetAlert && showSweetAlert)
                {
                    ViewBag.ShowSweetAlert = true;
                    TempData.Remove("ShowSweetAlert"); // Remove the flag to prevent showing again
                }
            }
            if (existingEmail == null)
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

            HttpContext.Session.SetString("Username", loggedUser);
            HttpContext.Session.SetString("IsLoggedIn", "true");

            TempData["SuccessMessage"] = "Welcome To Trabahub!  " + loggedUser;
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Credentials");
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

		[HttpGet]
		public IActionResult ForgotPassword(string? code)
		{
			var useremail = _context.Credentials.Where(s => s.Email == code).FirstOrDefault();

			return View(useremail);
		}



        [HttpPost]
        public IActionResult ForgotPassword(Credentials credentialsEdit)
        {
            var credential = _context.Credentials.FirstOrDefault(s => s.Email == credentialsEdit.Email);

            if (credential != null)
            {
                // Update the existing credential with the new password
                credential.Password = credentialsEdit.Password;
                _context.SaveChanges();
            }

            TempData["ForgotMessage"] = "Password Updated Successfully!";
            TempData["ShowSweetAlert"] = true;
            return View(credentialsEdit);
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

        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            bool emailExists = _context.Credentials.Any(s => s.Email == email);
            return Json(new { exists = emailExists });
        }


    }
}
