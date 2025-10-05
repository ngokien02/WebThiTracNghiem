using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = "Teacher")]
    public class ResultController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResultController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllKetQua(string? keyword, string? exam)
        {
            var query = _context.KetQua
                .Include(k => k.SinhVien)
                .Include(k => k.DeThi)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.ToLower();
                query = query.Where(k =>
                    (k.SinhVien.HoTen != null && k.SinhVien.HoTen.ToLower().Contains(keyword)) ||
                    (k.SinhVien.UserName != null && k.SinhVien.UserName.ToLower().Contains(keyword)) ||
                    (k.DeThi.TieuDe != null && k.DeThi.TieuDe.ToLower().Contains(keyword))
                );
            }

            if (!string.IsNullOrEmpty(exam))
                query = query.Where(k => k.DeThi.TieuDe == exam);

            var data = await query
                .Select(k => new
                {
                    MSSV = k.SinhVien.UserName,
                    HoTen = k.SinhVien.HoTen,
                    KyThi = k.DeThi.TieuDe,
                    NgayThi = k.GioLam.ToString("dd/MM/yyyy HH:mm"),
                    ThoiGianLamBai = k.GioNop.HasValue ? (k.GioNop.Value - k.GioLam).ToString(@"hh\:mm\:ss") : "N/A",
                    Diem = k.Diem,
                    KetQua = k.Diem >= 5 ? "Đạt" : "Không đạt"
                })
                .ToListAsync();

            return Json(data);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteKetQua(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "Id không hợp lệ." });

            var ketQua = await _context.KetQua.FindAsync(id);
            if (ketQua == null)
                return Json(new { success = false, message = "Không tìm thấy kết quả." });

            _context.KetQua.Remove(ketQua);

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa kết quả thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Xóa thất bại: " + ex.Message });
            }
        }
    } 
}
