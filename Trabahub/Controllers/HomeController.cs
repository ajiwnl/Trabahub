using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Trabahub.Data;
using Trabahub.Models;
using Trabahub.Helpers;
using Stripe;
using System.IO.Pipelines;

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

        public async Task<IActionResult> IndexAsync()
        {
            var getTotalUsers = _context.Credentials.Count();
            var getTotalListings = _context.Listing.Count();
            var getTotalInteractions = _context.ListInteraction.Count();
		var userType = HttpContext.Session.GetString("UserType");

            var getTotalPrice = _context.Analytics.FirstOrDefault();
            if (getTotalPrice != null)
            {
                double priceDouble = Convert.ToDouble(getTotalPrice.TotalIncome);
                HttpContext.Session.SetString("TotalCharges", "₱" + priceDouble.ToString());
            } else
            {
                HttpContext.Session.SetString("TotalCharges", "₱0");

            }

            // Access the CalculateTotalChargesFromStripe method
            var getTotalCharges = await StripeHelper.CalculateTotalChargesFromStripe();

            HttpContext.Session.SetString("TotalListing", getTotalListings.ToString());
            HttpContext.Session.SetString("TotalUsers", getTotalUsers.ToString());
            HttpContext.Session.SetString("TotalBooks", getTotalInteractions.ToString());

            if (userType == "Owner")
            {
              
                return View();
            }
            else
            {

                return View();
            }
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
