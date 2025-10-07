using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Models;
using Xceed.Words.NET;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Drawing;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Newtonsoft.Json;
using System.Security.Claims;
using WebThiTracNghiem.Areas.Admin.Models;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
	[Area("Teacher")]
	[Authorize(Roles = $"Admin, Teacher")]
	public class ExamController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public ExamController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_db = db;
			_roleManager = roleManager;
		}
		public IActionResult Index()
		{
			var cd = _db.ChuDe
					.Include(c => c.CauHoiList)
					.ThenInclude(ch => ch.DapAnList)
					.ToList();

			return PartialView("Index", cd);
		}
		public async Task<IActionResult> XuLyUpload(IFormFile examFile)
		{
			if (examFile == null || examFile.Length == 0)
			{
				return BadRequest("File không hợp lệ.");
			}

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
						string rawText = paragraph.Text;
						if (string.IsNullOrWhiteSpace(rawText)) continue;

						// Cắt nhỏ nếu có xuống dòng (Shift+Enter)
						var lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

						foreach (var line in lines)
						{
							string text = line.Trim();
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

							// Nhận diện đáp án
							if (currentCauHoi != null && answerIndex < 4 && Regex.IsMatch(text, @"^\*?[A-D]\."))
							{
								bool isCorrect = false;
								string content = text;

								if (content.StartsWith("*"))
								{
									isCorrect = true;
									content = content.Substring(1).Trim();
								}

								// Kiểm tra màu đỏ
								foreach (var run in paragraph.MagicText)
								{
									//if (run?.formatting?.FontColor.HasValue == true &&
									//	run.formatting.FontColor.Value.ToArgb() == ColorTranslator.FromHtml("#EE0000").ToArgb())
									//{
									//	isCorrect = true;
									//	break;
									//}
									if (run?.formatting?.FontColor.HasValue == true)
									{
										var color = run.formatting.FontColor.Value;
										if (color.R > 150 && color.G < 100 && color.B < 100)
										{
											isCorrect = true;
											break;
										}
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
							}
						}
					}

					if (currentCauHoi != null && currentCauHoi.DapAnList.Any())
					{
						cauHoiList.Add(currentCauHoi);
					}
				}
			}

			return PartialView("_DeThiUploaded", cauHoiList);
		}

		[HttpPost, ActionName("CreateExam")]
		public bool CreateExam()
		{
			try
			{
				var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
				var form = Request.Form;

				var idGiangVien = form["IdGiangVien"].ToString();
				if (idGiangVien != currentUserId)
				{
					return false;
				}

				// Tạo mới DeThi (KHÔNG gán Id từ form)
				var deThi = new DeThi
				{
					MaDe = form["MaDe"],
					TieuDe = form["TieuDe"],
					GioBD = DateTime.Parse(form["GioBD"]),
					GioKT = DateTime.Parse(form["GioKT"]),
					SoCauHoi = int.Parse(form["SoCauHoi"]),
					ThoiGian = int.TryParse(form["ThoiGian"], out var thoiGian) ? thoiGian : 0,
					IdGiangVien = idGiangVien,
					DiemToiDa = int.TryParse(form["DiemToiDa"], out var diem) ? diem : 10,
					RandomCauHoi = bool.TryParse(form["RandomCauHoi"], out var rc) && rc,
					RandomDapAn = bool.TryParse(form["RandomDapAn"], out var rd) && rd,
					ShowKQ = bool.TryParse(form["ShowKQ"], out var skq) && skq,
				};

				_db.DeThi.Add(deThi);
				_db.SaveChanges();

				// Lấy danh sách câu hỏi từ form
				var cauHoiListRaw = form["cauHoiObj"];
				var cauHoiList = JsonConvert.DeserializeObject<List<CauHoi>>(cauHoiListRaw);

				var tenCD = form["ChuDe"].ToString();
				var chuDe = _db.ChuDe.FirstOrDefault(cd => cd.TenCD == tenCD);

				var newCD = new ChuDe();
				if (chuDe == null)
				{
					newCD.TenCD = tenCD;
					_db.ChuDe.Add(newCD);
					_db.SaveChanges();
				}

				foreach (var cauHoi in cauHoiList)
				{
					var dapAnList = cauHoi.DapAnList;
					cauHoi.DapAnList = null;

					cauHoi.ChuDeId = chuDe != null ? chuDe.Id : newCD.Id;

					var existingQuestion = _db.CauHoi
						.Include(ch => ch.DapAnList)
						.FirstOrDefault(ch => ch.NoiDung == cauHoi.NoiDung
						&& ch.ChuDeId == cauHoi.ChuDeId);

					bool isSameDapAnList = existingQuestion != null
										&& existingQuestion.DapAnList.Count == dapAnList.Count
										&& !existingQuestion.DapAnList
											.Where((da, i) => da.NoiDung != dapAnList[i].NoiDung
															|| da.DungSai != dapAnList[i].DungSai)
											.Any();


					if (existingQuestion == null || !isSameDapAnList)
					{
						_db.CauHoi.Add(cauHoi);
						_db.SaveChanges();

						foreach (var dapAn in dapAnList)
						{
							dapAn.CauHoiId = cauHoi.Id;
							_db.DapAn.Add(dapAn);
							_db.SaveChanges();
						}
					}
					//luu cauhoi vao chitietdethi
					var chiTietDT = new ChiTietDeThi
					{
						DeThiId = deThi.Id,
						CauHoiId = (existingQuestion == null || !isSameDapAnList) ? cauHoi.Id : existingQuestion.Id
					};

					_db.ChiTietDeThi.Add(chiTietDT);
					_db.SaveChanges();
				}

				_db.SaveChanges();

				//tao thong bao admin
				var tbAdmin = new AdminNotification
				{
					LoaiTB = "TaoDe",
					TieuDe = deThi.TieuDe,
					GioTB = DateTime.Now
				};

				_db.ThongBaoAdmin.Add(tbAdmin);
				_db.SaveChanges();

				return true;
			}
			catch
			{
				return false;
			}
		}

		public IActionResult LoadQuestionFromBank(int? chuDeId)
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

			return PartialView("_ListQuestionFromBank", data);
		}

		[HttpGet]
		public async Task<IActionResult> GetEditExam(int id)
		{
			var currentUser = await _userManager.GetUserAsync(User);

			var deThi = await _db.DeThi.FirstOrDefaultAsync(d => d.Id == id);

			if (deThi == null)
				return NotFound();

			if (currentUser.Id.ToString() != deThi.IdGiangVien)
			{
				return Json(new
				{
					success = false,
					message = "Bạn không thể chỉnh sửa bài thi của người khác!"
				});
			}

			// Map dữ liệu cơ bản sang JSON, không cần câu hỏi
			var result = new
			{
				Id = deThi.Id,
				TieuDe = deThi.TieuDe,
				MaDe = deThi.MaDe,
				GioBD = deThi.GioBD.ToString("yyyy-MM-ddTHH:mm"),
				GioKT = deThi.GioKT.ToString("yyyy-MM-ddTHH:mm"),
				ThoiGian = deThi.ThoiGian,
				RandomCauHoi = deThi.RandomCauHoi,
				RandomDapAn = deThi.RandomDapAn,
				ShowKQ = deThi.ShowKQ
			};

			return Json(result);
		}
		[HttpPost]
		public IActionResult EditExam([FromBody] DeThi model)
		{
			if (model == null || model.Id <= 0)
				return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

			var deThi = _db.DeThi.FirstOrDefault(d => d.Id == model.Id);
			if (deThi == null)
				return Json(new { success = false, message = "Không tìm thấy đề thi." });

			// Cập nhật
			deThi.TieuDe = model.TieuDe;
			deThi.MaDe = model.MaDe;
			deThi.GioBD = model.GioBD;
			deThi.GioKT = model.GioKT;
			deThi.ThoiGian = model.ThoiGian;
			deThi.RandomCauHoi = model.RandomCauHoi;
			deThi.RandomDapAn = model.RandomDapAn;
			deThi.ShowKQ = model.ShowKQ;

			_db.SaveChanges();

			return Json(new { success = true, message = "Cập nhật đề thi thành công!" });
		}

		[HttpPost]
		public async Task<IActionResult> DeleteExam(int id)
		{
			var currentUser = await _userManager.GetUserAsync(User);

			var deThi = await _db.DeThi.FirstOrDefaultAsync(d => d.Id == id);

			if (deThi == null)
				return Json(new { success = false, message = "Không tìm thấy đề thi." });

			if (currentUser.Id.ToString() != deThi.IdGiangVien)
			{
				return Json(new
				{
					success = false,
					message = "Bạn không thể xóa bài thi của người khác!"
				});
			}

			if (_db.KetQua.Any(k => k.DeThiId == id))
			{
				return Json(new { success = false, message = "Không thể xóa vì đề thi đã có kết quả!" });
			}

			_db.DeThi.Remove(deThi);
			await _db.SaveChangesAsync();

			return Json(new { success = true, message = "Xóa đề thi thành công!" });
		}
	}
}