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

            // Check if the user is an owner
            if (userType == "Owner")
            {
                // Get the logged-in owner's username
                var ownerUsername = HttpContext.Session.GetString("Username");

                // Query the listings belonging to the logged-in owner
                var getTotalListings = _context.Listing.Count(l => l.OwnerUsername == ownerUsername);


                HttpContext.Session.SetString("TotalListing", getTotalListings.ToString());


                var ownerTotalIncome = _context.Analytics
                    .Where(a => a.DataReference == ownerUsername)
                    .Sum(a => a.TotalIncome);


                HttpContext.Session.SetString("TotalCharges", "₱" + ownerTotalIncome.ToString());
            }
            else
            {

                HttpContext.Session.SetString("TotalListing", "0");

                var totalIncomeForAllOwners = _context.Analytics.Sum(a => a.TotalIncome);

                HttpContext.Session.SetString("TotalCharges", "₱" + totalIncomeForAllOwners.ToString());
            }


            var getTotalUsers = _context.Credentials.Count();
            var getTotalInteractions = _context.ListInteraction.Count();

            HttpContext.Session.SetString("TotalUsers", getTotalUsers.ToString());
            HttpContext.Session.SetString("TotalBooks", getTotalInteractions.ToString());

            return View();
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