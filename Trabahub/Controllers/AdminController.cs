using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Trabahub.Data;

namespace Trabahub.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }
        public IActionResult Dashboard()
        {
            int countPending = _context.Listing.Where(p => p.ListingStatus == false).Count();
            int countVerified = _context.Listing.Where(v => v.ListingStatus == true).Count();

            if (countPending == 0) { HttpContext.Session.SetString("PendingCount", "0"); }
            else { HttpContext.Session.SetString("PendingCount", countPending.ToString()); }

            if (countVerified == 0) { HttpContext.Session.SetString("VerifiedCount", "0"); }
            else { HttpContext.Session.SetString("VerifiedCount", countVerified.ToString()); }

            var listings = _context.Listing.ToList();
            return View(listings);
        }

        [HttpGet]
        public IActionResult ListingDetails(string? name)
        {
            var listingDetail = _context.Listing.FirstOrDefault(x => x.ESTABNAME == name);
            var getOwner = listingDetail.OwnerUsername;

            var totalIncome = _context.Analytics.FirstOrDefault(owner => owner.DataReference == getOwner).TotalIncome;
            var referOwner = _context.Listing.FirstOrDefault(referowner => referowner.OwnerUsername == getOwner);
            int getBookCount = _context.Booking.Where(bookings => bookings.ESTABNAME == referOwner.ESTABNAME).Count();

            if (totalIncome != null) { HttpContext.Session.SetString("TotalIncome", totalIncome.ToString()); }
            else { HttpContext.Session.SetString("TotalIncome", "0"); }

            if (getBookCount == 0) { HttpContext.Session.SetString("OwnerBooks", "0"); }
            else { HttpContext.Session.SetString("OwnerBooks", getBookCount.ToString()); }

            return View(listingDetail);
        }

        [HttpPost]
        public IActionResult ApproveListing(string estabname, string action)
        {
            var findListing = _context.Listing.FirstOrDefault(x => x.ESTABNAME == estabname);

            if (action == "approve")
            {
                findListing.ListingStatus = true;
            }
            else if (action == "deny")
            {
                _context.Listing.Remove(findListing);
            }

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

    }
}
