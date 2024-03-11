using Microsoft.AspNetCore.Mvc;
using Trabahub.Data;
using Trabahub.Models;
using Microsoft.EntityFrameworkCore;


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
			var listings = _context.Listing.ToList();
            return View(listings);

        }

		[HttpGet]
		public IActionResult Index(string searchSpaces)
		{
			var spaces = from x in _context.Listing
						 select x;

			if (!string.IsNullOrEmpty(searchSpaces))
			{
				spaces = spaces.Where(x => EF.Functions.Like(x.ESTABNAME, $"%{searchSpaces}%"));
			}

			return View(spaces.ToList());
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

		private void SaveData(Listing addListing)
        {
            string imgPath = UploadFile(addListing);

            var listing = new Listing()
            {
                ESTABNAME = addListing.ESTABNAME,
                ESTABDESC = addListing.ESTABDESC,
                ESTABADD = addListing.ESTABADD,
                ESTABPRICE = addListing.ESTABPRICE,
                STARTTIME = addListing.STARTTIME,
                ENDTIME = addListing.ENDTIME,
                ESTABIMAGEPATH = imgPath,
                ESTABRATING = 1.5,
			};

            _context.Listing.Add(listing);
            _context.SaveChanges();
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
            string[] fieldNames = { "Establishment Name", "Establishment Description", "Establishment Address", "Price Rate", "Start Time", "End Time"};
   
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
            if (listing.ESTABPRICE == 0)
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
