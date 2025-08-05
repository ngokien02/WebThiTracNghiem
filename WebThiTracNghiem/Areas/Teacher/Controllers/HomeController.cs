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
        private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _root;
        public HomeController(ApplicationDbContext db, IWebHostEnvironment root)
        {
            _db = db;
            _root = root;
        }

        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ Giảng viên";
            return View();
        }
        public async Task<IActionResult> QuestionBank()
        {
            var cauHoiList = await _db.CauHoi
                .Include(q => q.DapAnList)
                .OrderByDescending(q => q.Loai)
                .ToListAsync();

            return PartialView("_QuestionBank", cauHoiList);
        }
		public IActionResult Reports()
		{

			var deThis = _db.DeThi
				.Include(d => d.GiangVien)
				.Include(d => d.KetQuaList)
				.AsNoTracking()
				.ToList();

			// Gom tất cả kết quả từ các đề
			var ketQuas = _db.KetQua
			.Include(k => k.SinhVien)
			.Include(k => k.DeThi)
			.AsNoTracking()
			.ToList();

			var diemList = ketQuas.Where(k => k.Diem.HasValue).Select(k => k.Diem.Value).ToList();
			var diemTB = diemList.Any() ? diemList.Average() : 0;
			var trenTB = diemList.Count(d => d > diemTB);
			var tyle = diemList.Count > 0 ? (trenTB * 100.0 / diemList.Count) : 0;
			var soNguoiThamGia = ketQuas.Select(k => k.IdSinhVien).Distinct().Count();
			ViewBag.ThongKe = new
			{
				TongNguoi = soNguoiThamGia,
				DiemTB = diemTB,
				TyLeTrenTB = tyle
			};

			return PartialView("_Reports", deThis);
		}
	}
}
