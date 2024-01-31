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
        public ActionResult SendEmail(string name, string email, string contact, string message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Customer Feedback");
                    var receiverEmail = new MailAddress("alvinjohnaranetz@gmail.com", "Receiver");
                    var password = "aiij duhw rnxy onwa";

                    // Format the subject using the provided inputs
                    var subject = $"Customer Feedback Contact: {name}_{email}-[{contact}]";

                    var body = message;

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
