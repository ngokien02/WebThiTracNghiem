using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class ExamController : Controller
    {

        private readonly ApplicationDbContext _db;
        public ExamController(ApplicationDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllExams(string keyword = "", string status = "")
        {
            var query = _db.DeThi.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(d => (d.TieuDe != null && d.TieuDe.Contains(keyword)) || (d.MaDe != null && d.MaDe.Contains(keyword)));

            var now = DateTime.Now;
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "running") query = query.Where(d => d.GioBD <= now && d.GioKT > now);
                else if (status == "upcoming") query = query.Where(d => d.GioBD > now);
                else if (status == "finished") query = query.Where(d => d.GioKT <= now);
            }

            var exams = await query.AsNoTracking().ToListAsync();
            return Json(exams);
        }

    }
}
