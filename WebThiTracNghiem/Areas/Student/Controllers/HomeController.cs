using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebThiTracNghiem.Areas.Student.Models;
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
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var listKetQua = await _db.KetQua
				.Join(_db.DeThi,
				kq => kq.DeThiId,
				dt => dt.Id,
				(kqdt, dt) => new
				{
					dt.MaDe,
					dt.TieuDe,
					kqdt.GioLam,
					dt.ThoiGian,
					dt.ShowKQ,
					kqdt.Diem,
					kqdt.TrangThai,
					kqdt.IdSinhVien
				})
                .Where(kqdt => kqdt.IdSinhVien == userId && kqdt.ShowKQ == true)      
                .OrderByDescending(kq => kq.GioLam)
                .ToListAsync();
            return PartialView("_History", listKetQua);
        }
    }
}
