using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text.RegularExpressions;
using WebThiTracNghiem.Models;
using Xceed.Words.NET;

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

		public async Task<IActionResult> QuestionBank(int? chuDeId)
		{
			var query = _db.ChuDe
					.Include(cd => cd.CauHoiList)
					.ThenInclude(cd => cd.DapAnList)
					.AsQueryable();

			if (chuDeId != null || chuDeId > 0)
			{
				query = query.Where(q => q.Id == chuDeId);
			}

			var data = query.ToList();

			return PartialView("_QuestionBank", data);
		}
		[HttpPost]
		public async Task<IActionResult> Import(IFormFile examFile)
		{
			try
			{
				if (examFile == null || examFile.Length == 0)
					return BadRequest("File không hợp lệ.");

				var cauHoiList = new List<CauHoi>();

				using (var stream = new MemoryStream())
				{
					await examFile.CopyToAsync(stream);
					stream.Position = 0;

					using (var doc = DocX.Load(stream))
					{
						var paragraphs = doc.Paragraphs;
						CauHoi currentCauHoi = null;
						char[] optionLetters = { 'A', 'B', 'C', 'D' };
						int answerIndex = 0;

						foreach (var paragraph in paragraphs)
						{
							string text = paragraph.Text.Trim();
							if (string.IsNullOrWhiteSpace(text)) continue;

							// Nhận diện câu hỏi
							var matchQuestion = Regex.Match(text, @"^\d+\.\s*(.+)");
							if (matchQuestion.Success)
							{
								if (currentCauHoi != null && currentCauHoi.DapAnList.Any())
									cauHoiList.Add(currentCauHoi);

								currentCauHoi = new CauHoi
								{
									NoiDung = matchQuestion.Groups[1].Value.Trim(),
									DapAnList = new List<DapAn>()
								};
								answerIndex = 0;
								continue;
							}

							// Xử lý đáp án A/B/C/D, chấp nhận có hoặc không dấu * đầu dòng
							if (currentCauHoi != null && answerIndex < 4 && Regex.IsMatch(text, @"^\*?[A-D]\."))
							{
								bool isCorrect = false;
								string content = text;

								// Kiểm tra dấu *
								if (content.StartsWith("*"))
								{
									isCorrect = true;
									content = content.Substring(1).Trim();
								}

								// Kiểm tra màu đỏ
								foreach (var run in paragraph.MagicText)
								{
									if (run?.formatting?.FontColor.HasValue == true &&
										run.formatting.FontColor.Value.ToArgb() == ColorTranslator.FromHtml("#EE0000").ToArgb())
									{
										isCorrect = true;
										break;
									}
								}

								// Loại bỏ tiền tố A./B./...
								content = Regex.Replace(content, @"^[A-D]\.", "").Trim();

								currentCauHoi.DapAnList.Add(new DapAn
								{
									NoiDung = content,
									DungSai = isCorrect
								});
								answerIndex++;
								continue;
							}
						}

						if (currentCauHoi != null && currentCauHoi.DapAnList.Any())
							cauHoiList.Add(currentCauHoi);
					}
				}

				return PartialView("_ImportedQuestions", cauHoiList);
			}
			catch (Exception ex)
			{
				return BadRequest("Có lỗi khi xử lý file: " + ex.Message);
			}
		}
		[HttpPost]
		public IActionResult SaveImported(string ChuDe, List<CauHoi> CauHoiList)
		{
			if (string.IsNullOrWhiteSpace(ChuDe) || CauHoiList == null || !CauHoiList.Any())
			{
				return Json(new { success = false, message = "Không có câu hỏi hoặc chủ đề!" });
			}

			// Kiểm tra hoặc tạo Chủ đề
			var chuDeEntity = _db.ChuDe.FirstOrDefault(cd => cd.TenCD == ChuDe);
			if (chuDeEntity == null)
			{
				chuDeEntity = new ChuDe { TenCD = ChuDe };
				_db.ChuDe.Add(chuDeEntity);
				_db.SaveChanges();
			}

			foreach (var cauHoi in CauHoiList)
			{
				var dapAnList = cauHoi.DapAnList; // Giữ danh sách đáp án
				cauHoi.Id = 0;
				cauHoi.ChuDeId = chuDeEntity.Id;
				cauHoi.DapAnList = null;

				// Kiểm tra xem câu hỏi đã tồn tại chưa
				var exists = _db.CauHoi.Any(ch => ch.NoiDung == cauHoi.NoiDung && ch.ChuDeId == chuDeEntity.Id);
				if (!exists)
				{
					_db.CauHoi.Add(cauHoi);
					_db.SaveChanges();

					// Lưu đáp án
					if (dapAnList != null)
					{
						foreach (var dapAn in dapAnList)
						{
							var newDapAn = new DapAn
							{
								NoiDung = dapAn.NoiDung,
								DungSai = dapAn.DungSai,
								CauHoiId = cauHoi.Id
							};
							_db.DapAn.Add(newDapAn);
						}
						_db.SaveChanges();
					}
				}
			}

			return Json(new { success = true, message = "Đã lưu vào Ngân hàng câu hỏi!" });
		}

		public IActionResult LoadPage(int page = 1)
		{
			int pageSize = 10;
			var totalResults = _db.KetQua.Count();

			var ketQuaList = _db.KetQua
				.Include(x => x.SinhVien)
				.Include(x => x.DeThi)
				.OrderByDescending(x => x.Id)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToList();

			ViewBag.TotalPages = (int)Math.Ceiling((double)totalResults / pageSize);
			ViewBag.CurrentPage = page;

			return View(ketQuaList);
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
		public IActionResult ActiveExam()
		{
			var danhSachDeThi = _db.DeThi
				.Where(dt => dt.GioKT > DateTime.Now)
				.Include(dt => dt.GiangVien)
				.OrderByDescending(dt => dt.GioBD)
				.ToList();

			return PartialView("_ActiveExam", danhSachDeThi);
		}
		public IActionResult Results()
		{
			var ketQuas = _db.KetQua
				.Include(k => k.SinhVien)
				.Include(k => k.DeThi)
				.Include(k => k.ChiTietKQs)
					.ThenInclude(ct => ct.CauHoi)
						.ThenInclude(ch => ch.DapAnList)
				.Include(k => k.ChiTietKQs)
					.ThenInclude(ct => ct.DapAnChons)
				.AsNoTracking()
				.ToList() ?? new List<KetQua>();

			return PartialView("_Results", ketQuas);
		}

		public IActionResult LoadQuestionByTopic(int? chuDeId)
		{
			var query = _db.ChuDe
				.Include(cd => cd.CauHoiList)
					.ThenInclude(ch => ch.DapAnList)
				.AsQueryable();

			if (chuDeId != null && chuDeId > 0)
			{
				query = query.Where(cd => cd.Id == chuDeId);
			}

			var data = query.ToList();

			return PartialView("_ListQuestionByTopic", data);
		}
	}
}
