using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebThiTracNghiem.Areas.Student.Models;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Student.Controllers
{
	[Area("Student")]
	[Authorize(Roles = "Student")]
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

		public async Task<IActionResult> History(int page = 1, int pageSize = 5, string keyword = "")
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Lấy danh sách kết quả của user, lọc theo từ khóa (tiêu đề hoặc mã đề)
			var query = _db.KetQua
				.Where(kq => kq.IdSinhVien == userId && kq.DeThi.ShowKQ)
				.Include(kq => kq.SinhVien)
				.Include(kq => kq.DeThi)
				.Include(kq => kq.ChiTietKQs)
					.ThenInclude(ct => ct.CauHoi)
						.ThenInclude(ch => ch.DapAnList)
				.Include(kq => kq.ChiTietKQs)
					.ThenInclude(ct => ct.DapAnChons)
				.AsNoTracking();

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(kq => kq.DeThi.TieuDe.Contains(keyword) || kq.DeThi.MaDe.Contains(keyword));
			}

			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			var listKetQua = await query
				.OrderByDescending(kq => kq.GioLam)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.Keyword = keyword;

			return PartialView("_History", listKetQua);
		}

		public IActionResult Guide()
		{
			ViewData["Title"] = "Trang hướng dẫn";
			return PartialView("_Guide");
		}
	}
}
