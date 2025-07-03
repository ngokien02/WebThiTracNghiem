using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Teacher.Controllers
{
    [Area("Teacher")]
	[Authorize(Roles = VaiTro.Role_Teach)]
	public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Trang chủ Giảng viên";
            return View();
        }
    }
}
