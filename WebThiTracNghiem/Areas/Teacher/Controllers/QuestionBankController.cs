using DocumentFormat.OpenXml.Office.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xceed.Words.NET;
using System.Drawing;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = $"Admin, Teacher")]
    public class QuestionBankController : Controller
    {
        private readonly ApplicationDbContext _db;

        public QuestionBankController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetEditQuestion(int id)
        {
            var cauHoi = await _db.CauHoi
                .Include(ch => ch.DapAnList)
                .Include(ch => ch.ChuDe)
                .FirstOrDefaultAsync(ch => ch.Id == id);

            if (cauHoi == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi." });

            return Json(new
            {
                id = cauHoi.Id,
                noiDung = cauHoi.NoiDung,
                chuDeId = cauHoi.ChuDeId,
                dapAnList = cauHoi.DapAnList.Select(d => new
                {
                    id = d.Id,
                    noiDung = d.NoiDung,
                    dungSai = d.DungSai
                })
            });
        }

        // 🟦 Lấy danh sách chủ đề để fill dropdown
        [HttpGet]
        public async Task<IActionResult> GetAllChuDe()
        {
            var chuDes = await _db.ChuDe
                .Select(cd => new { id = cd.Id, tenChuDe = cd.TenCD })
                .ToListAsync();

            return Json(chuDes);
        }

        // 🟨 Cập nhật câu hỏi + đáp án
        [HttpPost]
        public async Task<IActionResult> EditQuestion([FromBody] CauHoi model)
        {
            if (model == null)
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });

            var existingQuestion = await _db.CauHoi
                .Include(ch => ch.DapAnList)
                .FirstOrDefaultAsync(ch => ch.Id == model.Id);

            if (existingQuestion == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi." });

            // 🔹 Cập nhật thông tin cơ bản
            existingQuestion.NoiDung = model.NoiDung;
            existingQuestion.ChuDeId = model.ChuDeId;

            // 🔹 Cập nhật danh sách đáp án
            foreach (var dapan in model.DapAnList)
            {
                var existingAnswer = existingQuestion.DapAnList.FirstOrDefault(d => d.Id == dapan.Id);
                if (existingAnswer != null)
                {
                    existingAnswer.NoiDung = dapan.NoiDung;
                    existingAnswer.DungSai = dapan.DungSai;
                }
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật câu hỏi thành công!" });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteQuestionBank(int id)
        {
            if (id <= 0)
                return Json(new { success = false, message = "ID câu hỏi không hợp lệ." });

            var question = await _db.CauHoi
                .Include(q => q.DapAnList)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return Json(new { success = false, message = "Không tìm thấy câu hỏi." });

            // Xóa các đáp án trước (nếu có)
            if (question.DapAnList != null && question.DapAnList.Any())
            {
                _db.DapAn.RemoveRange(question.DapAnList);
            }

            // Xóa câu hỏi
            _db.CauHoi.Remove(question);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa câu hỏi thành công!" });
        }
        [HttpPost]
        public async Task<IActionResult> AddQuestion([FromBody] CauHoi model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.NoiDung) || model.ChuDeId <= 0)
                return Json(new { success = false, message = "Không có câu hỏi hoặc chủ đề!" });

            // Kiểm tra câu hỏi đã tồn tại chưa trong cùng chủ đề
            var exists = await _db.CauHoi.AnyAsync(ch => ch.NoiDung == model.NoiDung && ch.ChuDeId == model.ChuDeId);
            if (exists)
                return Json(new { success = false, message = "Câu hỏi đã tồn tại!" });

            var dapAnList = model.DapAnList; // giữ lại danh sách đáp án
            model.Id = 0; // đảm bảo EF tạo mới
            model.DapAnList = null; // tách ra để thêm riêng

            // Lưu câu hỏi mới
            _db.CauHoi.Add(model);
            await _db.SaveChangesAsync();

            // Lưu đáp án nếu có
            if (dapAnList != null && dapAnList.Any())
            {
                foreach (var dapAn in dapAnList)
                {
                    dapAn.Id = 0;
                    dapAn.CauHoiId = model.Id;
                    _db.DapAn.Add(dapAn);
                }
                await _db.SaveChangesAsync();
            }

            return Json(new { success = true, message = "Đã lưu câu hỏi vào Ngân hàng câu hỏi!" });
        }
        [HttpGet]
        [HttpGet]
        public IActionResult ExportFormat()
        {
            try
            {
                using (var doc = Xceed.Words.NET.DocX.Create("CauHoiMau.docx"))
                {
                    doc.InsertParagraph("Đáp án đúng có thể có * đằng trước hoặc tô đỏ")
                       .FontSize(12)
                       .Italic()
                       .Color(Xceed.Drawing.Color.Red)
                       .Alignment = Xceed.Document.NET.Alignment.left;
                    // Tạo 3 câu hỏi mẫu
                    var questions = new[]
                         {
                            new {
                                NoiDung = "1. Nội dung câu hỏi mẫu 1",
                                DapAn = new[] {
                                    new { Text = "A. Đây là đáp án đúng", UseRed = true, UseStar = false }, // đỏ
                                    new { Text = "B. Đây là đáp án sai", UseRed = false, UseStar = false },
                                    new { Text = "C. Đây là đáp án sai", UseRed = false, UseStar = false },
                                    new { Text = "D. Đây là đáp án sai", UseRed = false, UseStar = false }
                                }
                            },
                            new {
                                NoiDung = "3. Nội dung câu hỏi mẫu 2",
                                DapAn = new[] {
                                    new { Text = "A. Đây là đáp án sai", UseRed = false, UseStar = false },
                                    new { Text = "*B. Đây là đáp án đúng", UseRed = true, UseStar = false },
                                    new { Text = "C. Đây là đáp án sai", UseRed = false, UseStar = false },   
                                    new { Text = "D. Đây là đáp án sai", UseRed = false, UseStar = false }
                                }
                            },
                            new {
                                NoiDung = "2. Nội dung câu hỏi mẫu 3",
                                DapAn = new[] {
                                    new { Text = "A. Đây là đáp án đúng", UseRed = true, UseStar = true },  // có *
                                    new { Text = "*B. Đây là đáp án đúng", UseRed = true, UseStar = false },
                                    new { Text = "C. Đây là đáp án sai", UseRed = false, UseStar = false },
                                    new { Text = "D. Đây là đáp án sai", UseRed = false, UseStar = false }
                                }
                            }
                        };
                    foreach (var q in questions)
                    {
                        doc.InsertParagraph(q.NoiDung)
                           .FontSize(14)
                           .Bold();

                        // Các đáp án
                        foreach (var ans in q.DapAn)
                        {
                            string textToInsert = ans.UseStar ? "*" + ans.Text : ans.Text;
                            var p = doc.InsertParagraph(textToInsert)
                                       .FontSize(12);

                            if (ans.UseRed)
                                p.Color(Xceed.Drawing.Color.Red); // tô đỏ đáp án đúng
                        }

                        // Dòng trống giữa các câu hỏi
                        doc.InsertParagraph("");
                    }


                    using (var stream = new MemoryStream())
                    {
                        doc.SaveAs(stream);
                        stream.Position = 0;

                        return File(stream.ToArray(),
                                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                                    "CauHoiMau.docx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi khi tạo file mẫu: " + ex.Message);
            }
        }

    }
}