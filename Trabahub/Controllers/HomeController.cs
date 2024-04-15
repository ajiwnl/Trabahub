using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Trabahub.Data;
using Trabahub.Models;

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
            var getTotalUsers = _context.Credentials.Count();
            var getTotalListings = _context.Listing.Count();
            var getTotalInteractions = _context.ListInteraction.Count();

            HttpContext.Session.SetString("TotalListing", getTotalUsers.ToString());
            HttpContext.Session.SetString("TotalUsers", getTotalListings.ToString());
            HttpContext.Session.SetString("TotalBooks", getTotalInteractions.ToString());

            ViewData["ActivePage"] = "Home";
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