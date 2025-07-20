using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;
using WebThiTracNghiem.Areas.Student.Models;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Student.Controllers
{
	[Area("Student")]
	public class ExamController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly ILogger<ExamController> _logger;
		public ExamController(ApplicationDbContext db, ILogger<ExamController> logger)
		{
			_db = db;
			_logger = logger;
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

		[HttpGet]
		public async Task<IActionResult> StartExam(int id)
		{
			var deThi = await _db.DeThi
							.Include(d => d.CauHoiList)
							.ThenInclude(ch => ch.DapAnList)
							.FirstOrDefaultAsync(d => d.Id == id && d.GioKT >= DateTime.Now);

			if (deThi == null)
				return NotFound("Không tìm thấy đề thi.");

			var listCauHoi = deThi.RandomCauHoi
				? deThi.CauHoiList.OrderBy(c => Guid.NewGuid()).ToList()
				: deThi.CauHoiList.ToList();

			foreach (var ch in listCauHoi)
			{
				if (ch.DapAnList != null)
				{
					ch.DapAnList = deThi.RandomDapAn
						? ch.DapAnList.OrderBy(d => Guid.NewGuid()).ToList()
						: ch.DapAnList.ToList();
				}
			}

			var deThiVm = new DeThiViewModel
			{
				Id = deThi.Id,
				TieuDe = deThi.TieuDe,
				MaDe = deThi.MaDe,
				GioBD = deThi.GioBD,
				GioKT = deThi.GioKT,
				ThoiGian = deThi.ThoiGian,
				SoCauHoi = deThi.SoCauHoi,
				DiemToiDa = deThi.DiemToiDa,
				RandomCauHoi = deThi.RandomCauHoi,
				RandomDapAn = deThi.RandomDapAn,
				ShowKQ = deThi.ShowKQ,
				CauHoiList = listCauHoi.Select(ch => new CauHoiViewModel
				{
					Id = ch.Id,
					NoiDung = ch.NoiDung,
					Loai = ch.Loai,
					DapAnList = ch.DapAnList.Select(d => new DapAnViewModel
					{
						Id = d.Id,
						NoiDung = d.NoiDung,
						IsDung = d.DungSai,
						IsSelected = false
					}).ToList()
				}).ToList()
			};

			if (deThi.GioBD > DateTime.Now)
			{
				return Json(new { success = false, message = "Chưa đến giờ làm bài." });
			}

			var currentSession = new CurrentExamSession
			{
				DeThi = deThiVm,
				TimeStart = DateTime.Now,
				IsDone = false
			};

			HttpContext.Session.SetJson("currentExamSession", currentSession);

			ViewData["diem"] = deThi.SoCauHoi > 0
				? Math.Round(deThi.DiemToiDa / deThi.CauHoiList.Count, 1)
				: 0;

			return PartialView("_DoExam", deThiVm);
		}

		[HttpPost]
		public async Task<IActionResult> SaveQuestion([FromBody] SaveAnswerDto model)
		{

			if (model == null || model.CauHoiId <= 0)
			{
				return BadRequest("Dữ liệu không hợp lệ.");
			}

			var currentSession = HttpContext.Session.GetJson<CurrentExamSession>("currentExamSession");

			if (currentSession == null || currentSession.DeThi == null)
			{
				return BadRequest("Không tìm thấy phiên làm bài.");
			}

			var cauHoi = currentSession.DeThi.CauHoiList.FirstOrDefault(ch => ch.Id == model.CauHoiId);

			if (cauHoi == null)
			{
				return NotFound("Không tìm thấy câu hỏi trong đề thi.");
			}

			foreach (var dapAn in cauHoi.DapAnList)
			{
				dapAn.IsSelected = model.DapAnIds.Contains(dapAn.Id);
			}

			HttpContext.Session.SetJson("currentExamSession", currentSession);

			return Ok(new { success = true, message = "Cập nhật thành công." });
		}

		[HttpGet]
		public IActionResult RenderQuestion(int questionId)
		{
			var currentSession = HttpContext.Session.GetJson<CurrentExamSession>("currentExamSession");

			if (currentSession == null || currentSession.DeThi == null)
			{
				return BadRequest("Không tìm thấy phiên làm bài.");
			}

			var cauHoi = currentSession.DeThi.CauHoiList
				.FirstOrDefault(ch => ch.Id == questionId);

			if (cauHoi == null)
			{
				return NotFound("Không tìm thấy câu hỏi với ID đã cung cấp.");
			}

			var result = new
			{
				id = cauHoi.Id,
				noiDung = cauHoi.NoiDung,
				loai = cauHoi.Loai,
				dapAnList = cauHoi.DapAnList.Select(da => new
				{
					id = da.Id,
					noiDung = da.NoiDung,
					isSelected = da.IsSelected
				}).ToList()
			};

			return Json(result);
		}

		public async Task<IActionResult> SubmitExam()
		{
			var currentSession = HttpContext.Session.GetJson<CurrentExamSession>("currentExamSession");

			if (currentSession == null)
			{
				_logger.LogWarning("currentExamSession is null");
				return Json(new { success = false, message = "Không tìm thấy bài thi." });
			}

			int totalQuestions = currentSession.DeThi.CauHoiList.Count;
			int correctAnswers = 0;

			foreach (var cauHoi in currentSession.DeThi.CauHoiList)
			{
				bool isCorrect = false;

				if (cauHoi.Loai == "TracNghiem")
				{
					isCorrect = cauHoi.DapAnList.Any(d => d.IsDung && d.IsSelected) &&
								cauHoi.DapAnList.Where(d => d.IsDung == false).All(d => d.IsSelected == false);
				}
				else if (cauHoi.Loai == "NhieuDapAn")
				{
					isCorrect = cauHoi.DapAnList.All(d =>
						(d.IsDung && d.IsSelected) || (!d.IsDung && !d.IsSelected));
				}

				if (isCorrect)
				{
					correctAnswers++;
				}
			}

			double score = Math.Round((double)correctAnswers / totalQuestions * 10, 2);

			if (currentSession.DeThi.ShowKQ)
			{
				return Ok(new
				{
					success = true,
					message = "Nộp bài thành công!",
					title = currentSession.DeThi.TieuDe,
					username = User.Identity.Name,
					timeDone = DateTime.Now,
					correctAnswers = correctAnswers + " / " + totalQuestions,
					score = score.ToString()
				});
			}
			else
			{
				return Ok(new
				{
					success = true,
					message = "Nộp bài thành công!",
					title = currentSession.DeThi.TieuDe,
					username = User.Identity.Name,
					timeDone = DateTime.Now,
				});
			}

		}
	}
}
