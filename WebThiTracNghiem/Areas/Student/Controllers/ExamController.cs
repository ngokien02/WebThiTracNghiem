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
		public IActionResult StartExam(int id)
		{
			var deThi = _db.DeThi
				.Include(d => d.CauHoiList)
				.ThenInclude(ch => ch.DapAnList)
				.FirstOrDefault(d => d.Id == id && d.GioKT >= DateTime.Now);

			if (deThi == null)
			{
				return NotFound("Không tìm thấy đề thi.");
			}

			return PartialView("_DoExam", deThi);
		}
	}
}
