using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceRegistration.Core;
using ServiceRegistration.Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceRegistration.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IMyService _myService;

		public HomeController(ILogger<HomeController> logger, IMyService myService)
		{
			_logger = logger;
			_myService = myService;
		}

		public IActionResult Index()
		{
			ViewBag.SomeData = _myService.GetSomeData();
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
