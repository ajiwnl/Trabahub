﻿using Microsoft.AspNetCore.Mvc;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Net.Mail;
using System.Net;
using System.Threading;
using Trabahub.Helpers;

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

			IQueryable<Listing> spacesQuery;

			if (userType == "Owner")
			{
				spacesQuery = _context.Listing.Where(x => x.OwnerUsername == username);
			}
			else
			{
				spacesQuery = _context.Listing.AsQueryable();
			}

			if (!string.IsNullOrEmpty(searchSpaces))
			{
				spacesQuery = spacesQuery.Where(x => EF.Functions.Like(x.ESTABNAME, $"%{searchSpaces}%"));
			}

			var spaces = spacesQuery.ToList();
			return View(spaces);
		}


		[HttpGet]
		public IActionResult Add()
		{
			return View();
		}

        [HttpGet]
        public IActionResult Charge()
        {
            return View();
        }


        [HttpPost]
        [ActionName("Interact")]
        public IActionResult Interaction(ListInteraction addinteractList)
        {
			var getOwner = _context.Listing.FirstOrDefault(x => x.ESTABNAME == addinteractList.ESTABNAME);
            // Add the interaction to the database
            int totalCount = _context.ListInteraction.Count();
            var interact = new ListInteraction()
            {
                InteractId = totalCount + 1,
                ESTABNAME = addinteractList.ESTABNAME,
                Username = addinteractList.Username,
				OwnerUsername = getOwner.OwnerUsername,
                InteractComment = addinteractList.InteractComment,
                InteractRating = addinteractList.InteractRating
            };
            _context.ListInteraction.Add(interact);
            _context.SaveChanges();

            // Calculate average rating
            double? averageRating = _context.ListInteraction
                .Where(i => i.ESTABNAME == addinteractList.ESTABNAME && i.InteractRating != null)
                .Average(i => i.InteractRating);

            // Update ESTABRATING in Listing table with the calculated average
            var listingToUpdate = _context.Listing.FirstOrDefault(l => l.ESTABNAME == addinteractList.ESTABNAME);
            if (listingToUpdate != null)
            {
                listingToUpdate.ESTABRATING = averageRating ?? 0; // If averageRating is null, default to 0
                _context.SaveChanges();
            }

            return RedirectToAction("Index", "Listing");
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
			var listing = _context.Listing.FirstOrDefault(s => s.ESTABNAME == name);
			var interactions = _context.ListInteraction.Where(i => i.ESTABNAME == name).ToList();

			var viewModel = new ListingDetails
			{
				Listing = listing,
				Interactions = interactions
			};

			return View(viewModel);
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

		private int GetAccommodationCount(string establishmentName)
		{
			var listing = _context.Listing.FirstOrDefault(l => l.ESTABNAME == establishmentName);
			if (listing != null)
			{
				return listing.ACCOMODATION ?? 0; // Use null-coalescing operator to handle null values
			}
			return 0; // Return 0 if the establishment is not found or if accommodation count is not set
		}

		// Function to update accommodation count
		private void UpdateAccommodationCount(string establishmentName, int newCount)
		{
			var listing = _context.Listing.FirstOrDefault(l => l.ESTABNAME == establishmentName);
			if (listing != null)
			{
				listing.ACCOMODATION = newCount;
				_context.SaveChanges();
			}
		}

        private void UpdateTotalPrice(double stripePrice, string listingESTABNAME)
        {
            var ownerUsername = GetOwnerUsername(listingESTABNAME);
            if (ownerUsername != null)
            {
                var analytics = _context.Analytics.FirstOrDefault(a => a.DataReference == ownerUsername);
                if (analytics == null)
                {
                    var createAnalytics = new Analytics()
                    {
                        DataReference = ownerUsername,
                        TotalIncome = stripePrice
                    };
                    _context.Analytics.Add(createAnalytics);
                }
                else
                {
                    analytics.TotalIncome += stripePrice;
                }

                _context.SaveChanges();
            }
        }

        private string GetOwnerUsername(string listingESTABNAME)
        {
            var listing = _context.Listing.FirstOrDefault(l => l.ESTABNAME == listingESTABNAME);
            if (listing != null)
            {
                return listing.OwnerUsername;
            }
            return null;
        }



        [HttpPost]
		public IActionResult Charge(string stripeEmail, string stripeToken, string stripePrice, string stripeDescription, string timeinhid, string timeouthid, string dropdownChoice, string dynamicdate)
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
					var message = $"Payment successful for reserved co-working space: {stripeDescription}.\nTransaction ID: {BalanceTransactionId}";
					var sprice = Convert.ToDecimal(stripePrice);
					var phpPrice = string.Format("{0:C}", sprice);

					// Deduct accommodation count
					int accommodationCount = GetAccommodationCount(stripeDescription);
					// Decrement accommodation count
					// Deduct accommodation count based on pass type
					switch (dropdownChoice)
					{
						case "Hourly Price":
							UpdateAccommodationCount(stripeDescription, accommodationCount - 1);
							// Schedule task to restore accommodation after 1 hour
							ScheduleAccommodationRestore(stripeDescription, 1);
							break;
						case "Day Pass Price":
							UpdateAccommodationCount(stripeDescription, accommodationCount - 1); 
																								 // Schedule task to restore accommodation after 24 hours
							ScheduleAccommodationRestore(stripeDescription, 24);
							break;
						case "Weekly Pass Price":
							UpdateAccommodationCount(stripeDescription, accommodationCount - 1); 
																								  // Schedule task to restore accommodation after 7 days
							ScheduleAccommodationRestore(stripeDescription, 7 * 24);
							break;
						case "Monthly Pass Price":
							UpdateAccommodationCount(stripeDescription, accommodationCount - 1); 
																								   // Schedule task to restore accommodation after 30 days
							ScheduleAccommodationRestore(stripeDescription, 30 * 24);
							break;
						default:
							// Handle unrecognized pass type
							break;
					}

					// Send email to client
					SendEmail(userName, email, message, phpPrice, timeinhid, timeouthid, dropdownChoice, dynamicdate);

					// Send email to owner
					SendEmailToOwner(ownerEmail, userName, email, BalanceTransactionId, stripeDescription, phpPrice, timeinhid, timeouthid, dropdownChoice, dynamicdate);

					double priceDouble = Convert.ToDouble(stripePrice);
					UpdateTotalPrice(priceDouble, stripeDescription);

                    TempData["PaySuccess"] = "Successful Payment, Please check your email for more details";
                    TempData["EstablishmentName"] = stripeDescription;
                    return RedirectToAction("Charge", "Listing");
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
       

        private void ScheduleAccommodationRestore(string establishmentName, int durationInHours)
		{
			// Schedule a task to restore accommodation after the specified duration
			Task.Delay(TimeSpan.FromHours(durationInHours)).ContinueWith((task) =>
			{
				// Restore accommodation count
				int originalAccommodationCount = GetOriginalAccommodationCount(establishmentName);
				UpdateAccommodationCount(establishmentName, originalAccommodationCount);
			});
		}

		private int GetOriginalAccommodationCount(string establishmentName)
		{
			var listing = _context.Listing.FirstOrDefault(l => l.ESTABNAME == establishmentName);
			if (listing != null)
			{
				return listing.ACCOMODATION ?? 0;
			}
			return 0; // Return 0 if the establishment is not found or if original accommodation count is not set
		}

		// Method to send email to the owner
		public void SendEmailToOwner(string ownerEmail, string clientUsername, string clientEmail, string transactionId, string listingDescription, string phpPrice, string timeinhid, string timeouthid, string dropdownChoice, string dynamicdate)
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
				body += $"-----------------------------------\n";
				body += $"Subscription: {dropdownChoice}\n";
				body += $"Amount Received: ₱{amount}\n";
				body += $"Time In: {timeinhid}\n";
				body += $"Time Out: {timeouthid}\n";
				body += $"Reservation Date: {dynamicdate}\n\n";
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
		public void SendEmail(string name, string email, string message, string phpPrice, string timeinhid, string timeouthid, string dropdownChoice, string dynamicdate)
		{
			try
			{
				var senderEmail = new MailAddress("trabahubco@gmail.com", "Trabahub Reservation Confirmation");
				var receiverEmail = new MailAddress(email, "Receiver");
				var password = "weaz drul elrl bngg";

				var subject = $"Co-working Space Reservation Confirmation";

				// Ensure phpPrice contains only the amount without any currency symbols
				decimal amount = Convert.ToDecimal(phpPrice.Replace("$", "").Replace("₱", ""));

				var body = $"Username: {name}\nEmail: {email} \nTotal Charge: ₱{amount}\n";
				body += $"{message}\n";
				body += $"Subscription: {dropdownChoice}\n";
				body += $"Time In: {timeinhid}\n";
				body += $"Time Out: {timeouthid}\n";
				body += $"Reservation Date: {dynamicdate}\n\n";
				body += $"Please show this message to the reserved workspace for authentication.\n\n";
				body += $"Do no reply to this message.";
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
				ACCOMODATION = addListing.ACCOMODATION,
				ESTABIMAGEPATH = imgPath,
				ESTABRATING = 0,
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