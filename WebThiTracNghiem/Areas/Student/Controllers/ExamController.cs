using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public ExamController(ApplicationDbContext db, ILogger<ExamController> logger, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _logger = logger;
            _userManager = userManager;
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
            var kq = await _db.KetQua
                            .FirstOrDefaultAsync(k => k.DeThiId == id);

            if (kq?.TrangThai == "HoanThanh")
            {
                return Json(new
                {
                    success = false,
                    message = "Bạn đã làm bài thi này rồi!"
                });
            }

            var deThi = await _db.DeThi
                .Include(dt => dt.ChiTietDe)
                .FirstOrDefaultAsync(d => d.Id == id && d.GioKT >= DateTime.Now);

            if (deThi == null)
                return NotFound("Không tìm thấy đề thi.");

            var cauHoiQuery = _db.CauHoi.Include(ch => ch.DapAnList);

            var listCauHoi = new List<CauHoi>();

            foreach(var ct in deThi.ChiTietDe)
            {
                var cauHoi = cauHoiQuery.FirstOrDefault(ch => ch.Id == ct.CauHoiId);
                listCauHoi.Add(cauHoi);
            }

            listCauHoi = deThi.RandomCauHoi?
                listCauHoi.OrderBy(ch => Guid.NewGuid()).ToList()
                : listCauHoi.ToList();

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
                ? Math.Round(deThi.DiemToiDa / listCauHoi.Count, 1)
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

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

			// Lấy KetQua đã tạo lúc StartExam
			var kq = await _db.KetQua
						.Include(k => k.ChiTietKQs)
						.FirstOrDefaultAsync(k => k.DeThiId == currentSession.DeThi.Id && k.IdSinhVien == userId);

			if (kq == null)
			{
				// Nếu chưa có, tạo mới KetQua
				kq = new KetQua
				{
					IdSinhVien = userId,
					DeThiId = currentSession.DeThi.Id,
					GioLam = currentSession.TimeStart,
					TrangThai = "HoanThanh",
					SoCauDung = 0,
					Diem = 0
				};
				_db.KetQua.Add(kq);
				await _db.SaveChangesAsync();
			}

			int totalQuestions = currentSession.DeThi.CauHoiList.Count;
			int correctAnswers = 0;

			foreach (var cauHoiVm in currentSession.DeThi.CauHoiList)
			{
				bool isCorrect = false;

				if (cauHoiVm.Loai == "TracNghiem")
				{
					isCorrect = cauHoiVm.DapAnList.Any(d => d.IsDung && d.IsSelected) &&
								cauHoiVm.DapAnList.Where(d => !d.IsDung).All(d => !d.IsSelected);
				}
				else if (cauHoiVm.Loai == "NhieuDapAn")
				{
					isCorrect = cauHoiVm.DapAnList.All(d =>
						(d.IsDung && d.IsSelected) || (!d.IsDung && !d.IsSelected));
				}

				if (isCorrect) correctAnswers++;

				// Cập nhật ChiTietKQ
				var chiTiet = kq.ChiTietKQs.FirstOrDefault(ct => ct.CauHoiId == cauHoiVm.Id);
				if (chiTiet == null)
				{
					// Nếu chưa có chi tiết, tạo mới
					chiTiet = new ChiTietKQ
					{
						KetQuaId = kq.Id,
						CauHoiId = cauHoiVm.Id,
						//Diem = isCorrect ? (double)currentSession.DeThi.DiemToiDa / totalQuestions : 0,
						DapAnId = cauHoiVm.DapAnList.Where(d => d.IsSelected).Select(d => d.Id).FirstOrDefault()
					};
					_db.ChiTietKQ.Add(chiTiet);
				}
				else
				{
					// Cập nhật chi tiết hiện có
					//chiTiet.Diem = isCorrect ? (double)currentSession.DeThi.DiemToiDa / totalQuestions : 0;
					chiTiet.DapAnId = cauHoiVm.DapAnList.Where(d => d.IsSelected).Select(d => d.Id).FirstOrDefault();
				}
			}

			kq.SoCauDung = correctAnswers;
			kq.Diem = Math.Round((double)correctAnswers / totalQuestions * currentSession.DeThi.DiemToiDa, 2);
			kq.GioNop = DateTime.Now;
			kq.TrangThai = "HoanThanh";

			await _db.SaveChangesAsync();
			HttpContext.Session.Remove("currentExamSession");

			return Ok(new
			{
				success = true,
				message = "Nộp bài thành công!",
				correctAnswers = $"{correctAnswers} / {totalQuestions}",
				score = kq.Diem
			});
		}
	}
}
