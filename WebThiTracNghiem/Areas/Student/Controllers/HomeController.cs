using Microsoft.AspNetCore.Mvc;
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
			return View();
		}
	}
}
