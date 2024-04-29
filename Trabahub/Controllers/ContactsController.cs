using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;

namespace Trabahub.Controllers
{
    public class ContactsController : Controller
    {
        public IActionResult Index()
        {
            ViewData["ActivePage"] = "Contact Us";
            return View();
        }

        public ActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendEmail(string contact, string message)
        {

            var userType = HttpContext.Session.GetString("UserType");
            var userName = HttpContext.Session.GetString("Username");
            var fname = HttpContext.Session.GetString("fName");
            var lname = HttpContext.Session.GetString("lName");
            var email = HttpContext.Session.GetString("Email");
            var fullname = fname + " " + lname;

            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Customer Support");
                    var receiverEmail = new MailAddress("trabahubco@gmail.com", "Receiver");
                    var password = "weaz drul elrl bngg";


                    // Format the subject using the provided inputs
                    var subject = $"Trabahub Customer Support";

					var body = $"Name: {fullname}\n" +
                        $"Username: {userName}\n" +
                        $"User Type: {userType}" +
                        $"\nEmail: {email}" +
                        $"\nContact Number: {contact}" +
                        $"\n\n\n {message}";


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
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }

                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }

            return RedirectToAction("Index");
        }

    }
}
