using Microsoft.AspNetCore.Mvc;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Net.Mail;
using System.Net;
using System.Threading;
using Trabahub.Helpers;
using System.Globalization;
using RestSharp;
using Newtonsoft.Json;
using System.Collections.Generic;

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
				// Retrieve all verified listings for clients
				var verifiedListings = _context.Listing.Where(x => x.ListingStatus == true).ToList();
				return View(verifiedListings);
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

		[HttpGet]
		public IActionResult EditListing(string? name)
		{
			var getReference = _context.Listing.Where(s => s.ESTABNAME == name).FirstOrDefault();
			return View(getReference);
		}

		[HttpPost]
		public IActionResult EditListing(Listing listingEdit)
		{
			var existingListing = _context.Listing.FirstOrDefault(s => s.ESTABNAME == listingEdit.ESTABNAME);
			if (existingListing != null)
			{
				// Update properties of the existing listing with the values from the edited listing
				existingListing.ESTABDESC = listingEdit.ESTABDESC;
				existingListing.ESTABADD = listingEdit.ESTABADD;
				existingListing.ESTABHRPRICE = listingEdit.ESTABHRPRICE;
				existingListing.ESTABDAYPRICE = listingEdit.ESTABDAYPRICE;
				existingListing.ESTABWKPRICE = listingEdit.ESTABWKPRICE;
				existingListing.ESTABMONPRICE = listingEdit.ESTABMONPRICE;
				existingListing.ACCOMODATION = listingEdit.ACCOMODATION;

				_context.SaveChanges();
				TempData["UpMessage"] = "Listing Successfully Updated!";
				return RedirectToAction("Index");
			}
			return View(listingEdit);
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
			HttpContext.Session.SetString("CurrentListing", name);
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

		[HttpGet]
		public IActionResult BookingDetails()
		{
			var ownerUsername = HttpContext.Session.GetString("Username");
			var owner = _context.Listing.Where(owner => owner.OwnerUsername == ownerUsername).ToList();

			if (owner.Any())
			{
				var establishmentNames = owner.Select(o => o.ESTABNAME).ToList();
				var bookings = _context.Booking.Where(booking => establishmentNames.Contains(booking.ESTABNAME)).ToList();
				return View(bookings);
			}
			else
			{
				var bookings = _context.Booking.ToList();
				return View(bookings);
			}
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
				var count = _context.Analytics.Count();
				if (analytics == null)
				{
					var createAnalytics = new Analytics()
					{
						Id = count + 1,
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
		public async Task<IActionResult> ChargeGCash(string gcashprice, string gcashdesc, string gcashin, string gcashout, string gcashchoice, string gcashdynamic, string status)
		{
			var options = new RestClientOptions("https://api.paymongo.com/v1/checkout_sessions");
			var client = new RestClient(options);
			string userName = HttpContext.Session.GetString("Username");
			string localurl = "https://localhost:7074/Listing/Charge/";
			string deployedurl = "http://trabahubco.somee.com/Listing/Charge/";

			var successUrl = localurl + Uri.EscapeDataString(gcashdesc);
			int priceInt = Convert.ToInt32(gcashprice) * 100;
			string convertPrice = (priceInt / 100).ToString();
			string concatDesc = userName + " " + gcashchoice + " " + gcashin + " " + gcashout + " " + gcashdynamic;

			if (gcashprice == null || gcashdesc == null || gcashin == null || gcashout == null || gcashchoice == null || gcashdynamic  == null || status == null)
			{
                TempData["MissingFields"] = "Something Went Wrong! Please Enter Correct Fields And Try Booking Again";
                return RedirectToAction("Index", "Listing");
            }

            SaveBookDetails(gcashdesc, userName, convertPrice, gcashin, gcashout, gcashchoice, gcashdynamic);

            var requestBodyJson = JsonConvert.SerializeObject(new
            {
                data = new
                {
                    attributes = new
                    {
                        send_email_receipt = true,
                        show_description = true,
                        show_line_items = true,
                        description = concatDesc,
                        line_items = new[]
               {
                    new
                    {
                        currency = "PHP",
                        amount = priceInt,
                        description = gcashdesc,

                        quantity = 1,
                        name = gcashchoice,
                    }
                },
                        payment_method_types = new[] { "gcash" },
                        success_url = successUrl,
                    }
                }
            });

            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", "Basic c2tfdGVzdF9zdnBTYXFyNnBLMlRaTXlOSkhCVmI5Sng6");
            request.AddJsonBody(requestBodyJson, false);


            var response = await client.PostAsync(request);

            if (response.IsSuccessful)
            {
                var responseData = response.Content;

                double priceDouble = Convert.ToDouble(gcashprice);
                UpdateTotalPrice(priceDouble, gcashdesc);

                int accommodationCount = GetAccommodationCount(gcashdesc);
                switch (gcashchoice)
                {
                    case "Hourly Price":
                        UpdateAccommodationCount(gcashdesc, accommodationCount - 1);
                        ScheduleAccommodationRestore(gcashdesc, 1);
                        break;
                    case "Day Pass Price":
                        UpdateAccommodationCount(gcashdesc, accommodationCount - 1);
                        ScheduleAccommodationRestore(gcashdesc, 24);
                        break;
                    case "Weekly Pass Price":
                        UpdateAccommodationCount(gcashdesc, accommodationCount - 1);
                        ScheduleAccommodationRestore(gcashdesc, 7 * 24);
                        break;
                    case "Monthly Pass Price":
                        UpdateAccommodationCount(gcashdesc, accommodationCount - 1);
                        ScheduleAccommodationRestore(gcashdesc, 30 * 24);
                        break;
                    default:
                        break;
                }

                dynamic responseObject = JsonConvert.DeserializeObject(responseData);
                string checkoutUrl = responseObject.data.attributes.checkout_url;
                TempData["EstablishmentName"] = gcashdesc;
                return Redirect(checkoutUrl);
            }
            else
            {
                return RedirectToAction("Index", "Listing");
            }
        }

        [HttpPost]
		public IActionResult Charge(string stripeEmail, string stripeToken, string stripePrice, string stripeDescription, string timeinhid, string timeouthid, string dropdownChoice, string dynamicdate)
		{
            long price = Convert.ToInt32(stripePrice) * 100;
            var customers = new CustomerService();
			var charges = new ChargeService();

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

					SaveBookDetails(stripeDescription, userName, phpPrice, timeinhid, timeouthid , dropdownChoice, dynamicdate);

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

		public void SaveBookDetails(string estabName, string userName, string priceRate, string timein, string timeout, string ddchoice, string dynamicdate)
		{
			DateTime startTime, endTime, dynamicDate;

			// Parse time strings into DateTime objects
			if (!DateTime.TryParseExact(timein, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
			{
				// Handle invalid time format
			}

			if (!DateTime.TryParseExact(timeout, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
			{
				// Handle invalid time format
			}

			if (!DateTime.TryParseExact(dynamicdate, "ddd MMM dd yyyy HH:mm:ss 'GMT'K '(Philippine Standard Time)'", CultureInfo.InvariantCulture, DateTimeStyles.None, out dynamicDate))
			{
				// Handle invalid date format
			}

            DateTime calculatedStartTime, calculatedEndTime;


            int totalBooking = _context.Booking.Count();
			var bookingDetail = new Booking()
			{
				Id = totalBooking + 1,
				ESTABNAME = estabName,
				Username = userName,
				PriceRate = priceRate,
				STARTTIME = startTime,
				ENDTIME = endTime,
				SelectedOption = ddchoice,
				DynamicDate = dynamicDate,
				Status = "Active",
			};

            switch (ddchoice)
            {
                case "Hourly Price":
                    calculatedStartTime = startTime;
					bookingDetail.STARTTIME = calculatedStartTime;
                    break;
                case "Day Pass Price":
                    calculatedStartTime = dynamicDate.AddDays(1);
					bookingDetail.STARTTIME = dynamicDate;
					bookingDetail.ENDTIME = calculatedStartTime;

                    break;
                case "Weekly Pass Price":
                    calculatedStartTime = dynamicDate.AddDays(7);
                    bookingDetail.STARTTIME = dynamicDate;
                    bookingDetail.ENDTIME = calculatedStartTime;

                    break;
                case "Monthly Pass Price":
                    calculatedStartTime = dynamicDate.AddMonths(1);
                    bookingDetail.STARTTIME = dynamicDate;
                    bookingDetail.ENDTIME = calculatedStartTime;
                    break;
                // Add more cases for other pass types if needed
                default:
                    // Handle unsupported pass types
                    return;
            }

            _context.Booking.Add(bookingDetail);
			_context.SaveChanges();
		}


        public void SaveData(Listing addListing)
        {
            string imgPath = UploadFile(addListing);
            var veriPaths = new List<string>();
            var listingProperties = addListing.GetType().GetProperties();
            foreach (var property in listingProperties)
            {
                if (property.Name.StartsWith("VERIMG") && property.GetValue(addListing) is IFormFile verImg)
                {
                    if (int.TryParse(property.Name.Substring(6), out int index))
                    {
                        var veriPath = UploadFileVer(addListing, verImg, index);
                        veriPaths.Add(veriPath);
                    }
                }

            }
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
                ListingStatus = false,
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

            // Assign verification image paths
            for (int i = 0; i < veriPaths.Count; i++)
            {
                var propertyInfo = listing.GetType().GetProperty($"VERIMAGEPATH{i + 1}");
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(listing, veriPaths[i]);
                }
            }


            _context.Listing.Add(listing);
            _context.SaveChanges();

            // Log the successful addition of the listing
            Console.WriteLine("Listing added successfully.");
        }




        // Controller action to fetch daily analytics data
        public JsonResult GetDailyData()
        {
            // Example: Fetching daily data for the past week
            DateTime endDate = DateTime.Today;
            DateTime startDate = endDate.AddDays(-7);

            // Query database to fetch daily analytics data
            var dailyData = _context.DailyAnalytics
                                .Where(d => d.Date >= startDate && d.Date <= endDate)
                                .OrderBy(d => d.Date)
                                .Select(d => new { Date = d.Date.ToShortDateString(), TotalUsers = d.TotalUsers })
                                .ToList();
			Console.WriteLine(dailyData.ToString());

			var role = HttpContext.Session.GetString("UserType");

			if(role == "Owner")
			{
                var ownerUsername = HttpContext.Session.GetString("Username");
                var getTotalListings = _context.Listing.Count(l => l.OwnerUsername == ownerUsername);
                var getOwner = _context.Listing.FirstOrDefault(a => a.OwnerUsername == ownerUsername);
                int getTotalBooks = 0;
                if (getOwner != null)
                {
                    getTotalBooks = _context.Booking.Count(x => x.ESTABNAME == getOwner.ESTABNAME);
                }

                var chartData = new
                {
                    DailyData = dailyData,
                    TotalListings = getTotalListings,
                    TotalBooks = getTotalBooks
                };
                return Json(chartData);
            }
           
			else if(role == "Admin")
			{
                var getTotalListings = _context.Listing.Count();
                var getTotalBooks = _context.ListInteraction.Count();

                var chartData = new
                {
                    DailyData = dailyData,
                    TotalListings = getTotalListings,
                    TotalBooks = getTotalBooks
                };
                return Json(chartData);
            }
			else
			{
				return Json(null);
			}
        }

		public JsonResult UpdateCount()
		{
			var currentListing = HttpContext.Session.GetString("CurrentListing");
			var listing = _context.Listing.FirstOrDefault(a => a.ESTABNAME == currentListing);

			if (listing != null)
			{
				// Check and update the booking status if needed
				var activeBookings = _context.Booking.Where(b => b.ESTABNAME == currentListing && b.ENDTIME < DateTime.Now);

				foreach (var booking in activeBookings)
				{
					if (booking.ENDTIME <= DateTime.Now && booking.Status != "Finished")
					{
						// Increase the accommodation count
						listing.ACCOMODATION++;
						booking.Status = "Finished";
					}
				}
				_context.SaveChanges();
			}

			return Json(listing?.ACCOMODATION ?? 0);
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

        private string UploadFileVer(Listing addListing, IFormFile file, int index)
        {
            string fileName = null;
            if (file != null)
            {
                string estabname = addListing.ESTABNAME.ToString();
                // Combine the elements to create the file name
                fileName = $"VerPhoto_{estabname}_{index}{Path.GetExtension(file.FileName)}";

                string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "verify");
                string filePath = Path.Combine(uploadDir, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }
            }
            return fileName;
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