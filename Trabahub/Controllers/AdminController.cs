﻿using Microsoft.AspNetCore.Mvc;
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
            var listings = _context.Listing.ToList();
            return View(listings);
        }

        [HttpGet]
        public IActionResult ListingDetails(string? name)
        {
            var listingDetail = _context.Listing.FirstOrDefault(x => x.ESTABNAME == name);
            return View(listingDetail);
        }

        [HttpPost]
        public IActionResult ApproveListing(string estabname)
        {
            var findListing = _context.Listing.FirstOrDefault(x => x.ESTABNAME == estabname);
            findListing.ListingStatus = true;
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
    }
}
