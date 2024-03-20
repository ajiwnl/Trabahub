using Microsoft.AspNetCore.Mvc;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Net.Mail;
using System.Net;

namespace Trabahub.Controllers
{
	public class ListingController : Controller
	{
		private readonly ApplicationDbContext _context;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ListingController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
		{
			_webHostEnvironment = webHostEnvironment;
			_context = context;
		}

		public IActionResult Index()
		{
			var userType = HttpContext.Session.GetString("UserType");
			var username = HttpContext.Session.GetString("Username");

			if (userType == "Owner")
			{
				// Retrieve listings only for the current owner
				var ownerListings = _context.Listing.Where(l => l.OwnerUsername == username).ToList();
				return View(ownerListings);
			}
			else
			{
				// Retrieve all listings for clients
				var allListings = _context.Listing.ToList();
				return View(allListings);
			}
		}

		[HttpGet]
		public IActionResult Index(string searchSpaces)
		{
			var userType = HttpContext.Session.GetString("UserType");
			var username = HttpContext.Session.GetString("Username");

			if (userType == "Owner")
			{
				var spaces = _context.Listing.Where(x => x.OwnerUsername == username);

				if (!string.IsNullOrEmpty(searchSpaces))
				{
					spaces = spaces.Where(x => EF.Functions.Like(x.ESTABNAME, $"%{searchSpaces}%"));
				}

				return View(spaces.ToList());
			}
			else
			{
				var allListings = _context.Listing.ToList();

				if (!string.IsNullOrEmpty(searchSpaces))
				{
					allListings = allListings.Where(x => EF.Functions.Like(x.ESTABNAME, $"%{searchSpaces}%")).ToList();
				}

				return View(allListings);
			}
		}

		[HttpGet]
		public IActionResult Add()
		{
			return View();
		}

		[HttpPost]
		[ActionName("Add")]
		public IActionResult Add(Listing addListing)
		{
			string errorMsg = FieldValidation(addListing);
			if (!string.IsNullOrEmpty(errorMsg))
			{
				// Contains the error message to TempData from the FieldValidation function,
				//this will be use to display in JavaScript Pop-up box
				TempData["FieldError"] = errorMsg;
				return View("Add", addListing);
			}

			if (_context.Listing.Any(s => s.ESTABNAME == addListing.ESTABNAME))
			{
				// The record already exists, this will be use to display in JavaScript Pop-up box
				TempData["DuplicateError"] = "Space(s) already in-list";

				// Return the view with the model validation errors
				return View(addListing);
			}
			SaveData(addListing);
			TempData["SuccessMessage"] = "Space Listed Successfully";
			return RedirectToAction("Index", "Listing");
		}

		[HttpGet]
		public IActionResult Details(string? name)
		{
			var listing = _context.Listing.Where(s => s.ESTABNAME == name).FirstOrDefault();
			return View(listing);
		}

		[HttpGet]
		public IActionResult Booking(string? name)
		{
			var booking = _context.Listing.Where(s => s.ESTABNAME == name).FirstOrDefault();

			var prices = new Dictionary<string, int>();

			if (booking.ESTABHRPRICE.HasValue)
			{
				prices.Add("Hourly Price", booking.ESTABHRPRICE.Value);
			}

			if (booking.ESTABDAYPRICE.HasValue)
			{
				prices.Add("Day Pass Price", booking.ESTABDAYPRICE.Value);
			}

			if (booking.ESTABWKPRICE.HasValue)
			{
				prices.Add("Weekly Pass Price", booking.ESTABWKPRICE.Value);
			}

			if (booking.ESTABMONPRICE.HasValue)
			{
				prices.Add("Monthly Pass Price", booking.ESTABMONPRICE.Value);
			}

			ViewBag.Prices = prices;

			return View(booking);
		}

		[HttpPost]
		public IActionResult Charge(string stripeEmail, string stripeToken, string stripePrice, string stripeDescription)
		{
			var customers = new CustomerService();
			var charges = new ChargeService();
			long price = Convert.ToInt32(stripePrice) * 100;

			try
			{
				var customer = customers.Create(new CustomerCreateOptions
				{
					Email = stripeEmail,
					Source = stripeToken
				});

				var charge = charges.Create(new ChargeCreateOptions
				{
					Amount = price,
					Description = stripeDescription,
					Currency = "php",
					Customer = customer.Id
				});

				if (charge.Status == "succeeded")
				{
					string BalanceTransactionId = charge.BalanceTransactionId;

					string userName = HttpContext.Session.GetString("Username");
					string ownerEmail = GetOwnerEmail(stripeDescription); // Get owner's email based on the listing

					var email = stripeEmail;
					var message = $"Payment successful for reserved co-working space: {stripeDescription}.\nTransaction ID: {BalanceTransactionId} \n\nPlease show this message to the reserved workspace for authentication. \n\n Do no reply to this message.";
					var sprice = Convert.ToDecimal(stripePrice);
					var phpPrice = string.Format("{0:C}", sprice);

					// Send email to client
					SendEmail(userName, email, message, phpPrice);

					// Send email to owner
					SendEmailToOwner(ownerEmail, userName, email, BalanceTransactionId, stripeDescription, phpPrice);

					TempData["PaySuccess"] = "Successful Payment, Please check your email for more details";
					return RedirectToAction("Index", "Listing");
				}
				else
				{
					TempData["PayFail"] = "Payment Failed, Please try again!";
					return RedirectToAction("Index", "Listing");
				}
			}
			catch (StripeException stripeEx)
			{
				// Handle specific Stripe exceptions
				if (stripeEx.StripeError.Type == "card_error" && stripeEx.StripeError.Code == "card_declined")
				{
					TempData["PayFail"] = "Card declined. Please check your card details and try again.";
					return RedirectToAction("Index", "Listing");
				}
				else
				{
					// Handle other Stripe exceptions
					TempData["PayFail"] = "Payment Failed. Please try again later.";
					return RedirectToAction("Index", "Listing");
				}
			}
			catch (Exception ex)
			{
				// Handle general exceptions
				TempData["PayFail"] = "Payment Failed. Please try again later.";
				return RedirectToAction("Index", "Listing");
			}
		}

		// Method to retrieve owner's email based on the listing ESTABNAME
		private string GetOwnerEmail(string listingESTABNAME)
		{
			var listing = _context.Listing.FirstOrDefault(l => l.ESTABNAME == listingESTABNAME);
			if (listing != null)
			{
				var ownerUsername = listing.OwnerUsername;
				var ownerCredentials = _context.Credentials.FirstOrDefault(c => c.Username == ownerUsername);
				if (ownerCredentials != null)
				{
					return ownerCredentials.Email;
				}
			}
			return null;
		}


		// Method to send email to the owner
		public void SendEmailToOwner(string ownerEmail, string clientUsername, string clientEmail, string transactionId, string listingDescription, string phpPrice)
		{
			try
			{
				var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Space Reservation Owner");
				var receiverEmail = new MailAddress(ownerEmail, "Owner");
				var password = "weaz drul elrl bngg";

				var subject = $"Co-working Space Reservation Notification";

				// Ensure phpPrice contains only the amount without any currency symbols
				decimal amount = Convert.ToDecimal(phpPrice.Replace("$", "").Replace("₱", ""));

				var body = $"A client has made a payment for the reservation of the co-working space: {listingDescription}.\n";
				body += $"Client Username: {clientUsername}\n";
				body += $"Client Email: {clientEmail}\n";
				body += $"Transaction ID: {transactionId}\n";
				body += $"Amount Received: ₱{amount}\n\n";
				body += $" Owner's Copy. Do no reply to this message";

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
			}
			catch (Exception)
			{
				ViewBag.Error = "Some Error";
			}
		}


		// Email sending logic
		public void SendEmail(string name, string email, string message, string phpPrice)
		{
			try
			{
				var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Reservation Confirmation");
				var receiverEmail = new MailAddress(email, "Receiver");
				var password = "weaz drul elrl bngg";

				var subject = $"Co-working Space Reservation Confirmation";

				// Ensure phpPrice contains only the amount without any currency symbols
				decimal amount = Convert.ToDecimal(phpPrice.Replace("$", "").Replace("₱", ""));

				var body = $"Username: {name}\nEmail: {email} \nTotal Charge: ₱{amount}\n {message}";

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
			}
			catch (Exception)
			{
				ViewBag.Error = "Some Error";
			}
		}
		public void SaveData(Listing addListing)
		{
			string imgPath = UploadFile(addListing);

			// Retrieve the username of the currently logged-in owner
			var ownerUsername = HttpContext.Session.GetString("Username");

			System.Diagnostics.Debug.WriteLine($"Owner username from session: {ownerUsername}");

			// Check if owner username is not null or empty
			if (string.IsNullOrEmpty(ownerUsername))
			{
				Console.WriteLine("Owner username is null or empty.");
				return;
			}

			var listing = new Listing()
			{
				ESTABNAME = addListing.ESTABNAME,
				ESTABDESC = addListing.ESTABDESC,
				ESTABADD = addListing.ESTABADD,
				STARTTIME = addListing.STARTTIME,
				ENDTIME = addListing.ENDTIME,
				ESTABIMAGEPATH = imgPath,
				ESTABRATING = 1.5,
				OwnerUsername = ownerUsername
			};

			// Check if ESTABHRPRICE is provided and assign it
			if (addListing.ESTABHRPRICE.HasValue)
			{
				listing.ESTABHRPRICE = addListing.ESTABHRPRICE.Value;
			}

			// Check if ESTABDAYPRICE is provided and assign it
			if (addListing.ESTABDAYPRICE.HasValue)
			{
				listing.ESTABDAYPRICE = addListing.ESTABDAYPRICE.Value;
			}

			// Check if ESTABWKPRICE is provided and assign it
			if (addListing.ESTABWKPRICE.HasValue)
			{
				listing.ESTABWKPRICE = addListing.ESTABWKPRICE.Value;
			}

			// Check if ESTABMONPRICE is provided and assign it
			if (addListing.ESTABMONPRICE.HasValue)
			{
				listing.ESTABMONPRICE = addListing.ESTABMONPRICE.Value;
			}

			_context.Listing.Add(listing);
			_context.SaveChanges();

			// Log the successful addition of the listing
			Console.WriteLine("Listing added successfully.");
		}

		private string UploadFile(Listing addListing)
		{
			string fileName = null;
			if (addListing.ESTABIMG != null)
			{
				string estabname = addListing.ESTABNAME.ToString();


				// Combine the elements to create the file name
				fileName = $"{estabname}{Path.GetExtension(addListing.ESTABIMG.FileName)}";

				string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
				string filePath = Path.Combine(uploadDir, fileName);
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					addListing.ESTABIMG.CopyTo(fileStream);
				}
			}
			return fileName;
		}

		private string UploadFile2(Listing addListing)
		{
			string fileName2 = null;
			if (addListing.ESTABIMG != null)
			{
				string estabname = addListing.ESTABNAME.ToString();


				// Combine the elements to create the file name
				fileName2 = $"VerPhoto_{estabname}{Path.GetExtension(addListing.ESTABIMG.FileName)}";

				string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "img");
				string filePath = Path.Combine(uploadDir, fileName2);
				using (var fileStream = new FileStream(filePath, FileMode.Create))
				{
					addListing.ESTABIMG.CopyTo(fileStream);
				}
			}
			return fileName2;
		}

		private string FieldValidation(Listing listing)
		{
			List<string> missingFields = new List<string>();
			string[] fieldNames = { "Establishment Name", "Establishment Description", "Establishment Address", "Price Rate", "Start Time", "End Time" };

			if (string.IsNullOrEmpty(listing.ESTABNAME))
			{
				missingFields.Add(fieldNames[1]);
			}
			if (string.IsNullOrEmpty(listing.ESTABDESC))
			{
				missingFields.Add(fieldNames[2]);
			}
			if (string.IsNullOrEmpty(listing.ESTABADD))
			{
				missingFields.Add(fieldNames[3]);
			}
			if (listing.ESTABHRPRICE == 0)
			{
				missingFields.Add(fieldNames[4]);
			}
			if (listing.STARTTIME == DateTime.MinValue)
			{
				missingFields.Add(fieldNames[5]);
			}
			if (listing.ENDTIME == DateTime.MinValue)
			{
				missingFields.Add(fieldNames[6]);
			}

			if (missingFields.Count > 0)
			{
				string fields = string.Join(", ", missingFields);
				string message = $"The following fields are missing or invalid values: {fields}";
				return message; // Return the no input in fields error message
			}
			return null; // No errors, return null

		}
	}
}