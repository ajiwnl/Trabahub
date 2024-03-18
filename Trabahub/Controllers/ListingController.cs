using Microsoft.AspNetCore.Mvc;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.EntityFrameworkCore;
using Stripe;

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
			return View(booking);
		}
		[HttpPost]
		public IActionResult Charge(string stripeEmail, string stripeToken, string estabPrice, string estabName)
		{
			var customers = new CustomerService();
			var charges = new ChargeService();
			var price = (long)Convert.ToDouble(estabPrice);

			var customer = customers.Create(new CustomerCreateOptions
			{
				Email = stripeEmail,
				Source = stripeToken
			});

			var charge = charges.Create(new ChargeCreateOptions
			{
				Amount = price,
				Description = estabName,
				Currency = "php",
				Customer = customer.Id
			});

			if (charge.Status == "succeeded")
			{
				string BalanceTransactionId = charge.BalanceTransactionId;
				return View();
			}
			else
			{

			}
			return View();
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

			// Check if ESTABYRPRICE is provided and assign it
			if (addListing.ESTABYRPRICE.HasValue)
			{
				listing.ESTABYRPRICE = addListing.ESTABYRPRICE.Value;
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