using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly UserManager<ApplicationUser> _userManager;

		public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
		{
			_logger = logger;
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			var hoTen = user?.HoTen;

			if (User.Identity.IsAuthenticated)
			{
				var roles = await _userManager.GetRolesAsync(user);
				if (roles.Contains(VaiTro.Role_Admin))
					return RedirectToAction("Index", "Home", new { area = "Admin" });

				if (roles.Contains(VaiTro.Role_Teach))
					return RedirectToAction("Index", "Home", new { area = "Teacher" });

				if (roles.Contains(VaiTro.Role_Stu))
					return RedirectToAction("Index", "Home", new { area = "Student" });
			}

			ViewBag.HoTen = hoTen;
			@ViewData["Title"] = "Trang chủ";
			return View();
		}

		[Authorize]
		public async Task<IActionResult> Guide()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return Unauthorized();

			var roles = await _userManager.GetRolesAsync(user);

			if (roles.Contains(VaiTro.Role_Admin))
				return PartialView("~/Areas/Admin/Views/Home/_Guide.cshtml");

			if (roles.Contains(VaiTro.Role_Teach))
				return PartialView("~/Areas/Teacher/Views/Home/_Guide.cshtml");

			if (roles.Contains(VaiTro.Role_Stu))
				return PartialView("~/Areas/Student/Views/Home/_Guide.cshtml");
			else
			{
				return View();
			}
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
