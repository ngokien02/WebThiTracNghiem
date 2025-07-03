using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Models;
using Xceed.Words.NET;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Drawing;
using System.Text.RegularExpressions;
using Xceed.Document.NET;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = VaiTro.Role_Teach)]
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ExamController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult CreateExam()
        {
            return PartialView("Index");
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
                    char[] optionLetters = new[] { 'A', 'B', 'C', 'D' };
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

                            // Kiểm tra và loại bỏ dấu *
                            if (content.StartsWith("*"))
                            {
                                isCorrect = true;
                                content = content.Substring(1).Trim(); // Bỏ dấu *
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

                            // Bỏ tiền tố A./B./... sau khi xử lý *
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
                    {
                        cauHoiList.Add(currentCauHoi);
                    }
                }
            }

            return PartialView("_DeThiUploaded", cauHoiList);
        }
    }
}
