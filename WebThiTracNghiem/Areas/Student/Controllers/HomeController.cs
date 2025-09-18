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
                            .Where(kq => kq.IdSinhVien == userId && kq.DeThi.ShowKQ)
                            .Include(kq => kq.SinhVien)
                            .Include(kq => kq.DeThi)
                            .Include(kq => kq.ChiTietKQs)
                                .ThenInclude(ct => ct.CauHoi)
                                    .ThenInclude(ch => ch.DapAnList)
                            .Include(kq => kq.ChiTietKQs)
                                .ThenInclude(ct => ct.DapAnChons)
                            .OrderByDescending(kq => kq.GioLam)
                            .AsNoTracking()
                            .ToListAsync();

            return PartialView("_History", listKetQua);
        }
    }
}
