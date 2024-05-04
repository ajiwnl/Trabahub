using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Trabahub.Data;
using Trabahub.Models;
using Trabahub.Helpers;
using Stripe;
using System.IO.Pipelines;
using Newtonsoft.Json;

namespace Trabahub.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public IActionResult Index()
        {
            // Get the logged-in user's type
            var userType = HttpContext.Session.GetString("UserType");
            var getRecentBooks = _context.Booking.ToList();

            // Check if the user is an owner
            if (userType == "Owner")
            {
                // Get the logged-in owner's username
                var ownerUsername = HttpContext.Session.GetString("Username");

                // Query the listings belonging to the logged-in owner
                var getTotalListings = _context.Listing.Count(l => l.OwnerUsername == ownerUsername);
				var getOwner = _context.Listing.FirstOrDefault(a => a.OwnerUsername == ownerUsername);
				if (getOwner == null)
				{
					// Handle the case where no listing is found for the owner
					// For example, set TotalBooks to 0
					HttpContext.Session.SetString("TotalBooks", "0");
				}
				else
				{
					// Listing is found, proceed with getting TotalBooks
					var getTotalBooks = _context.ListInteraction.Count(x => x.OwnerUsername == ownerUsername);
					HttpContext.Session.SetString("TotalBooks", getTotalBooks.ToString());
				}

				var getTotalUsers = _context.Credentials.Count();

				var ownerTotalIncome = _context.Analytics
					.Where(a => a.DataReference == ownerUsername)
					.Sum(a => a.TotalIncome);


				HttpContext.Session.SetString("TotalListing", getTotalListings.ToString());
				HttpContext.Session.SetString("TotalUsers", getTotalUsers.ToString());
				HttpContext.Session.SetString("TotalCharges", "₱" + ownerTotalIncome.ToString());

            }
            else if(userType == "Admin")
            {
                var getTotalListings = _context.Listing.Count();
                var getTotalUsers = _context.Credentials.Count();
                var getAllBooking = _context.Booking.Count();
                var totalIncomeForAllOwners = _context.Analytics.Sum(a => a.TotalIncome);

                HttpContext.Session.SetString("TotalListing", getTotalListings.ToString());
                HttpContext.Session.SetString("TotalUsers", getTotalUsers.ToString());
                HttpContext.Session.SetString("TotalBooks", getAllBooking.ToString());
                HttpContext.Session.SetString("TotalCharges", "₱" + totalIncomeForAllOwners.ToString());
            }

            return View(getRecentBooks);
        }


        public IActionResult Privacy()
        {
            ViewData["ActivePage"] = "Community";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}