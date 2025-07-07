using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Student.Controllers
{
	[Area("Student")]
	public class ExamController : Controller
	{
		private readonly ApplicationDbContext _db;
		public ExamController(ApplicationDbContext db)
		{
			_db = db;
		}
		public IActionResult Index()
		{
			var dsDeThi = _db.DeThi
				.Where(d => d.GioKT >= DateTime.Now)
				.OrderByDescending(d => d.GioBD)
				.Include(d => d.GiangVien)
				.ToList();
			return PartialView("Index", dsDeThi);
		}
	}
}
