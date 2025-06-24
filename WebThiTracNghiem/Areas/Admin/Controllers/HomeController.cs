using Microsoft.AspNetCore.Mvc;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			ViewData["Title"] = "Trang Quản trị";
			return View();
		}
	}
}
