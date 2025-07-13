using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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

		public async Task<IActionResult> Index()
		{
			var dsDeThi = await _db.DeThi
				.Where(d => d.GioKT >= DateTime.Now)
				.OrderByDescending(d => d.GioBD)
				.Include(d => d.GiangVien)
				.ToListAsync();

			return PartialView("Index", dsDeThi);
		}

		public async Task<IActionResult> StartExam(int id)
		{
			var deThi = await _db.DeThi
				.Include(d => d.CauHoiList)
					.ThenInclude(ch => ch.DapAnList)
				.FirstOrDefaultAsync(d => d.Id == id && d.GioKT >= DateTime.Now);

			if (deThi == null)
			{
				return NotFound("Không tìm thấy đề thi.");
			}

			var listCauHoiRanDom = deThi.RandomCauHoi ?
				deThi.CauHoiList.OrderBy(c => Guid.NewGuid()).ToList()
				: deThi.CauHoiList.ToList();

			foreach (var ch in listCauHoiRanDom)
			{
				if (ch.DapAnList != null)
				{
					ch.DapAnList = deThi.RandomDapAn ?
						ch.DapAnList.OrderBy(c => Guid.NewGuid()).ToList()
						: ch.DapAnList.ToList();
				}
			}

			deThi.CauHoiList = listCauHoiRanDom;

			HttpContext.Session.SetJson("currentExamSession", new
			{
				DeThi = deThi,
				TimeStart = DateTime.Now,
				IsDone = false
			});

			ViewData["diem"] = deThi.SoCauHoi > 0 ? Math.Round(deThi.DiemToiDa / deThi.CauHoiList.Count, 1) : 0;

			return PartialView("_DoExam", deThi);
		}

		public async Task<IActionResult> LoadQuestion(int index)
		{
			var currentExam = HttpContext.Session.GetJson<dynamic>("currentExamSession");
			var cauHoiList = ((JObject)currentExam.DeThi).ToObject<DeThi>().CauHoiList;
			var question = cauHoiList.ToList()[index];

			return Json(question);
		}
	}
}
