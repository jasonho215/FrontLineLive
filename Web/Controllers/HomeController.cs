﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Infrastructure;
using Web.Models;
using Web.Snippets;
using Web.Snippets.Messaging;
using Web.Db;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Web.Snippets.System.Collections.Generic;

namespace Web.Controllers
{
	[FrontEndFilter]
	public class HomeController : BaseController
	{
		public IActionResult Index([FromServices] DataContext dataContext)
		{
			SimpleNotifier noty = notifier();
            noty.AddMessage(MsgTypes.Information, "TIP: Try changing Map Layers (top right)");

			IQueryable<Need> allNeedsPosts = dataContext.Needs.Include(n => n.NeedPpeTypes);

			//This logic is intentionally "fluffy", we are (justifiably) bumping up the Met and Partially Met percentages 
			//by excluding posts from the "total" which volunteers cannot possibly meet: see notes on IsNotMarkedAllNotMetAndIsContactable etc

			int countAllNeedsPosts = allNeedsPosts.Count();
			int countSeeminglyContactableNeedsPosts = allNeedsPosts.ToList().Count(n => n.IsNotMarkedAllNotMetAndIsContactable());

			IQueryable<Need> fullyMetNeeds = allNeedsPosts.Where(p => p.NeedPpeTypes.All(pt => pt.StatusId == (int)PpeStatus.Met));
			int countFullyMetNeeds = fullyMetNeeds.Count();
			int countPartiallyMetNeeds = allNeedsPosts.Except(fullyMetNeeds).Count(p => p.NeedPpeTypes.Any(pt => pt.StatusId == (int)PpeStatus.Met));

			ViewData["met"] = countFullyMetNeeds;
			ViewData["partial"] = countPartiallyMetNeeds;
			ViewData["new"] = countSeeminglyContactableNeedsPosts - (countFullyMetNeeds + countPartiallyMetNeeds);

			ViewData["contactable"] = countSeeminglyContactableNeedsPosts;
			ViewData["noncontactable"] = countAllNeedsPosts - countSeeminglyContactableNeedsPosts;

			return View();
		}

		public IActionResult About() => View();
        public IActionResult Resources() => View();
        public IActionResult Reports() => View();

        [HttpGet("terms-and-conditions")]
		public IActionResult TermsAndConditions() => View();
        public IActionResult Media() => View();

		public IActionResult Intro() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}
	}
}