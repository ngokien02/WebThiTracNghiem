using Microsoft.AspNetCore.Mvc;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ Giảng viên";
            return View();
        }
    }
}
