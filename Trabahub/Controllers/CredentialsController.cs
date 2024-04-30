using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

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

        

        public IActionResult RedirectPage()
        {

            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            // Get the username of the currently logged-in user from session
            var username = HttpContext.Session.GetString("Username");
            var email = HttpContext.Session.GetString("Email");
            var userType = HttpContext.Session.GetString("UserType");

            // Fetch profile data for the currently logged-in user from the database
            var profile = _context.Credentials.FirstOrDefault(c => c.Email == email);

            ViewData["Username"] = username;
            ViewData["UserType"] = userType;

            // Pass profile data to the view
            return View(profile);
        }


        public IActionResult Admin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Admin(string adminemail, string adminpassword)
        {
            var checkAdmin = _context.Credentials.FirstOrDefault(x => x.Email == adminemail);
            if (checkAdmin == null || checkAdmin.Password != adminpassword)
            {
                return View("Login");
            }

            string loggedUser = checkAdmin.Username.ToString();
            string userType = checkAdmin.UserType.ToString();
            HttpContext.Session.SetString("Username", loggedUser);
            HttpContext.Session.SetString("UserType", userType);


            return RedirectToAction("Dashboard", "Admin");
        }
        [HttpPost]
        [ActionName("Login")]
        public IActionResult Login(string emaillog, string passwordlog)
        {
            var existingEmail = _context.Credentials.FirstOrDefault(x => x.Email == emaillog);

            if (existingEmail == null)
            {
                TempData["UserNull"] = "Email Not Found";
                return View("Login");
            }

            // Skip password verification for trabahubco@gmail.com
            if (existingEmail.Email == "trabahubco@gmail.com")
            {
                string loggedUser1 = existingEmail.Username.ToString();
                string userType1 = existingEmail.UserType.ToString();
                HttpContext.Session.SetString("Username", loggedUser1);
                HttpContext.Session.SetString("UserType", userType1);

                TempData["SuccessMessage"] = $"Welcome To Trabahub! {loggedUser1} ({userType1})";
                return RedirectToAction("Index", "Home"); // Redirect to home
            }

            // Verify the hashed password
            if (!VerifyPassword(passwordlog, existingEmail.Password))
            {
                TempData["PasswordIncorrect"] = "Incorrect Password";
                return View("Login");
            }

            string loggedUser = existingEmail.Username.ToString();
            string userType = existingEmail.UserType.ToString();
            HttpContext.Session.SetString("Username", loggedUser);
            HttpContext.Session.SetString("UserType", userType);
            HttpContext.Session.SetString("fName", existingEmail.fName);
            HttpContext.Session.SetString("lName", existingEmail.lName);
            HttpContext.Session.SetString("Email", emaillog);

            // Check user type and redirect accordingly
            if (userType == "Owner" || userType == "Client")
            {
                TempData["SuccessMessage"] = $"Welcome To Trabahub! {loggedUser} ({userType})";
                return RedirectToAction("Index", "Home"); // Redirect to home
            }
            else
            {
                // Handle unknown user type
                TempData["ErrorMessage"] = "Unknown User Type";
                return RedirectToAction("Index", "Home"); // Redirect to the home page or any other appropriate action
            }
        }

        public IActionResult Edit(string? email)
        { 
            var useremail = _context.Credentials.Where(s => s.Email == email).FirstOrDefault();

            return View(useremail);
        }

        [HttpPost]
        public IActionResult Edit(Credentials profileEdit)
        {
            var profile = _context.Credentials.FirstOrDefault(s => s.Email == profileEdit.Email);

            if (profile != null)
            {
      
                profile.Username = profileEdit.Username;
                profile.fName = profileEdit.fName;
                profile.lName = profileEdit.lName;


                _context.SaveChanges();
                TempData["UpMessage"] = "Profile Updated Successfully!";
                return RedirectToAction("Profile");
            }
            return NotFound();
        }




        // Method to verify the password
        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] salt = Convert.FromBase64String(hashedPassword.Split(':')[0]); //Convert salt as part of the hashed password
            byte[] storedHash = Convert.FromBase64String(hashedPassword.Split(':')[1]); // Extract the hash portion from the stored hashed password

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8);

            return Enumerable.SequenceEqual(storedHash, hash);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Credentials");
        }

        [HttpPost]
        public IActionResult Register(Credentials addCredentialEntry, string radBtnOptions)
        {
            // Check if model state is valid
            if (!ModelState.IsValid)
            {
                return View("Register", addCredentialEntry);
            }

            // Check if email already exists
            var existingEmail = _context.Credentials.FirstOrDefault(x => x.Email == addCredentialEntry.Email);
            if (existingEmail != null)
            {
                TempData["EmailExists"] = "Email Already Exists";
                return RedirectToAction("Register");
            }

            // Check if username already exists
            var existingUser = _context.Credentials.FirstOrDefault(x => x.Username == addCredentialEntry.Username);
            if (existingUser != null)
            {
                TempData["UserExists"] = "Username Already Exists";
                return RedirectToAction("Register");
            }

            try
            {
                // Generate salt
                byte[] salt = GenerateSalt();

                // Hash the password with salt before saving it
                string hashedPassword = HashPassword(addCredentialEntry.Password, salt);
                addCredentialEntry.Password = hashedPassword;

                // Save the entered credentials to the database
                var today = DateTime.Today;
                var dailyAnalyticsEntry = _context.DailyAnalytics.FirstOrDefault(da => da.Date == today);
                int countEntry = _context.DailyAnalytics.Count();

                if (dailyAnalyticsEntry != null)
                {
                    dailyAnalyticsEntry.TotalUsers++;
                }
                else
                {
                    // If there's no entry for today, create a new one
                    var newDailyEntry = new DailyAnalytics
                    {
                        Id = countEntry + 1,
                        Date = today,
                        TotalUsers = 1 // Start with 1 for the new user
                    };
                    _context.DailyAnalytics.Add(newDailyEntry);
                    _context.SaveChanges();
                }

                addCredentialEntry.UserType = radBtnOptions;
                addCredentialEntry.CreationDate = DateTime.Today;
                SaveEntry(addCredentialEntry);

                TempData["SuccessMessage"] = "Registration Successful! You may now login to your account.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Failed to send verification email. Please try again later.";
                // Handle exception (log error, etc.)
                return RedirectToAction("Register");
            }

            // Redirect to a success page or perform any other desired action
            return RedirectToAction("Login");
        }

        // Method to generate a random salt
        public byte[] GenerateSalt()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // Method to hash the password with salt
        public string HashPassword(string password, byte[] salt)
        {
            string saltString = Convert.ToBase64String(salt);
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return $"{saltString}:{hashedPassword}"; // Concatenate the salt and hash with a colon separator
        }

        // Modified method to send verification email with a different name to avoid conflicts
        private void SendEmailWithVerificationCode(string email, string verificationCode)
        {
            try
            {
                var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Verification");
                var receiverEmail = new MailAddress(email, "Receiver");
                var password = "weaz drul elrl bngg";

                var subject = "Verification Code";
                var body = $"Your verification code is: {verificationCode}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };

                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(mess);
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error sending verification email: " + ex.Message);
            }
        }


        [HttpPost]
        public IActionResult VerifyRegistration(string verificationCode)
        {
            var savedVerificationCode = TempData["VerificationCode"]?.ToString();
            var credentials = TempData["Credentials"] as Credentials;

            if (verificationCode == savedVerificationCode && credentials != null)
            {
                // Save the user to the database
                SaveEntry(credentials);

                Console.WriteLine($"Verification code: {verificationCode}");
            }
            else
            {
                Console.WriteLine("Verification code is incorrect. Please try again.");
            }

            return View();
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
                try
                {
                    // Generate a new salt
                    byte[] salt = GenerateSalt();

                    // Hash the new password with the generated salt
                    string hashedPassword = HashPassword(credentialsEdit.Password, salt);

                    // Update the existing credential with the new hashed password
                    credential.Password = hashedPassword;

                    // Save changes to the database
                    _context.SaveChanges();

                    TempData["ForgotMessage"] = "Password Updated Successfully!";
                    TempData["ShowSweetAlert"] = true;
                }
                catch (Exception ex)
                {
                    // Handle exception (log error, etc.)
                    TempData["ForgotMessage"] = "Failed to update password. Please try again later.";
                    TempData["ShowSweetAlert"] = true;
                }
            }
            else
            {
                TempData["ForgotMessage"] = "User not found. Please enter a valid email address.";
                TempData["ShowSweetAlert"] = true;
            }

            return View(credentialsEdit);
        }


        // Method to save entry to the database
        public void SaveEntry(Credentials addCredentialEntry)
        {
            _context.Credentials.Add(addCredentialEntry);
            _context.SaveChanges();
        }

        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            bool emailExists = _context.Credentials.Any(s => s.Email == email);
            return Json(new { exists = emailExists });
        }

        // Method to generate a random verification code
        private string GenerateVerificationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }



        [HttpPost]
        [ActionName("SendEmail")]
        // Method to send verification email and return the verification code
        public IActionResult SendEmail(string email)
        {
            string verificationCode = GenerateVerificationCode();

            try
            {
                var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Verification");
                var receiverEmail = new MailAddress(email, "Receiver");
                var password = "weaz drul elrl bngg";

                var subject = "Verification Code";
                var body = $"Your Trabahub verification code is: {verificationCode}";

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail.Address, password)
                };

                using (var mess = new MailMessage(senderEmail, receiverEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(mess);
                }

                // Return the verification code
                return Ok(new { verificationCode });
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine("Error sending verification email: " + ex.Message);
                return StatusCode(500, "Failed to send verification email");
            }
        }

    }
}
