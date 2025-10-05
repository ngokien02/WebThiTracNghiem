using Microsoft.AspNetCore.Authorization;
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
	[Authorize(Roles = "Student")]
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

		public async Task<IActionResult> Index(int page = 1, string keyword = "")
		{
			int pageSize = 6;

			var query = _db.DeThi
				.Include(d => d.GiangVien)
				.Where(d => d.GioKT >= DateTime.Now);

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				query = query.Where(d => EF.Functions.Like(d.TieuDe, $"%{keyword}%"));
			}

			query = query.OrderByDescending(d => d.GioBD);

			var totalExams = await query.CountAsync();
			var exams = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync();

			ViewBag.TotalPages = (int)Math.Ceiling((double)totalExams / pageSize);
			ViewBag.CurrentPage = page;
			return PartialView("Index", exams);
		}

		[HttpGet]
        public async Task<IActionResult> StartExam(int id)
        {
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var kq = await _db.KetQua
                            .AsNoTracking()
                            .Where(kq => kq.IdSinhVien == user)
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

            foreach (var ct in deThi.ChiTietDe)
            {
                var cauHoi = cauHoiQuery.FirstOrDefault(ch => ch.Id == ct.CauHoiId);
                listCauHoi.Add(cauHoi);
            }

            listCauHoi = deThi.RandomCauHoi ?
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

            int totalQuestions = currentSession.DeThi.CauHoiList.Count;
            int correctAnswers = 0;

            var user = await _userManager.GetUserAsync(User);

            var kq = new KetQua
            {
                IdSinhVien = user?.Id,
                DeThiId = currentSession.DeThi.Id,
                GioLam = currentSession.TimeStart,
                GioNop = DateTime.Now,
                TrangThai = "HoanThanh",
                ChiTietKQs = new List<ChiTietKQ>()
            };

            foreach (var cauHoi in currentSession.DeThi.CauHoiList)
            {
                bool isCorrect = false;

                if (cauHoi.Loai == "TracNghiem")
                {
                    isCorrect = cauHoi.DapAnList.Any(d => d.IsDung && d.IsSelected) &&
                                cauHoi.DapAnList.Where(d => !d.IsDung).All(d => !d.IsSelected);
                }
                else if (cauHoi.Loai == "NhieuDapAn")
                {
                    isCorrect = cauHoi.DapAnList.All(d =>
                        (d.IsDung && d.IsSelected) || (!d.IsDung && !d.IsSelected));
                }

                if (isCorrect) correctAnswers++;

                var chiTiet = new ChiTietKQ
                {
                    CauHoiId = cauHoi.Id,
                    Diem = isCorrect ? (double)currentSession.DeThi.DiemToiDa / totalQuestions : 0,
                    DapAnChons = new List<ChiTietKQ_DapAn>()
                };

                foreach (var dapAn in cauHoi.DapAnList.Where(d => d.IsSelected))
                {
                    chiTiet.DapAnChons.Add(new ChiTietKQ_DapAn
                    {
                        DapAnId = dapAn.Id
                    });
                }

                kq.ChiTietKQs.Add(chiTiet);
            }

            kq.SoCauDung = correctAnswers;
            kq.Diem = Math.Round((double)correctAnswers / totalQuestions * currentSession.DeThi.DiemToiDa, 2);

            _db.KetQua.Add(kq);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("currentExamSession");

            if (currentSession.DeThi.ShowKQ)
            {
                return Ok(new
                {
                    success = true,
                    message = "Nộp bài thành công!",
                    title = currentSession.DeThi.TieuDe,
                    username = user?.HoTen,
                    timeDone = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                    correctAnswers = $"{correctAnswers} / {totalQuestions}",
                    score = kq.Diem
                });
            }
            else
            {
                return Ok(new
                {
                    success = true,
                    message = "Nộp bài thành công!",
                    title = currentSession.DeThi.TieuDe,
                    username = user?.HoTen,
                    timeDone = DateTime.Now.ToString("HH:mm dd/MM/yyyy"),
                });
            }
        }

    }
}
