using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Student.Controllers
{
	[Area("Student")]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hosting;
		public HomeController(ApplicationDbContext db, IWebHostEnvironment hosting)
		{
			_db = db;
			_hosting = hosting;
		}
		public IActionResult Index()
		{
			ViewData["Title"] = "Trang chủ Sinh viên";
			return View();
		}
	}
}
