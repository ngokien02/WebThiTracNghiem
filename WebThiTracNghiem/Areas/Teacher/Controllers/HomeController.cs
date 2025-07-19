using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
	[Area("Teacher")]
	[Authorize(Roles = VaiTro.Role_Teach)]
	public class HomeController : Controller
	{
		private readonly IWebHostEnvironment _hosting;

		private readonly ApplicationDbContext _db;
		public HomeController(ApplicationDbContext db, IWebHostEnvironment hosting)
		{
			_db = db;
			_hosting = hosting;
		}


		public IActionResult Index()
		{
			ViewData["Title"] = "Trang chủ Giảng viên";
			return View();
		}
		public async Task<IActionResult> Active()
		{
			var dsDeThi = await _db.DeThi
				.Where(d => d.GioKT >= DateTime.Now)
				.OrderByDescending(d => d.GioBD)
				.Include(d => d.GiangVien)
				.ToListAsync();
			var danhSachDeThi = _db.DeThi.ToList();
			return PartialView(dsDeThi);
		}
        public async Task<IActionResult> QuestionBank()
        {
            var cauHoiList = await _db.CauHoi
             .Include(q => q.DapAnList) // Bao gồm đáp án nếu có navigation
             .OrderByDescending(q => q.NoiDung)
             .ToListAsync();
            return PartialView(cauHoiList); // hoặc View(), nếu bạn muốn load layout đầy đủ
        }

    }
}
