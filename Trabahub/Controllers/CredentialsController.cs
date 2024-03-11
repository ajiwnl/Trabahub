using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
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

		public IActionResult RedirectPage()
		{

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

			if (existingEmail.Password != passwordlog)
			{
				TempData["PasswordIncorrect"] = "Incorrect Password";
				return View("Login");
			}

			string loggedUser = existingEmail.Username.ToString();
			string userType = existingEmail.UserType.ToString(); // Assuming you have a property called UserType in your Credentials model

			HttpContext.Session.SetString("Username", loggedUser);
			HttpContext.Session.SetString("UserType", userType);

			// Check user type and redirect accordingly
			if (userType == "Owner")
			{
				TempData["SuccessMessage"] = "Welcome To Trabahub! " + loggedUser + " (Owner)";
				return RedirectToAction("Index", "Home"); // Redirect to home
			}
			else if (userType == "Client")
			{
				TempData["SuccessMessage"] = "Welcome To Trabahub! " + loggedUser + " (Client)";
				return RedirectToAction("Index", "Home"); // Redirect to home
            }
			else
			{
				// Handle unknown user type
				TempData["ErrorMessage"] = "Unknown User Type";
				return RedirectToAction("Index", "Home"); // Redirect to the home page or any other appropriate action
			}
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
				// If model state is invalid, return the registration view with validation errors
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
				// Save the entered credentials to the database
				addCredentialEntry.UserType = radBtnOptions;
				SaveEntry(addCredentialEntry);

				TempData["SuccessMessage"] = "Registration Successful! You may know Login to your account.";
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
				// Update the existing credential with the new password
				credential.Password = credentialsEdit.Password;
				_context.SaveChanges();
			}

			TempData["ForgotMessage"] = "Password Updated Successfully!";
			TempData["ShowSweetAlert"] = true;
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
