﻿using Microsoft.AspNetCore.Mvc;

namespace Trabahub.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
