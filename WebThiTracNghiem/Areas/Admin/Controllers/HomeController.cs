using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class HomeController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public HomeController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public async Task<IActionResult> Index()
		{
			var result = new Dictionary<string, int>();
			var roles = _roleManager.Roles.ToList();

			foreach (var role in roles)
			{
				var usersInRole = await _userManager.GetUsersInRoleAsync(VaiTro.Role_Teach);
				result[VaiTro.Role_Teach] = usersInRole.Count;
			}

			foreach (var role in roles)
			{
				var usersInRole = await _userManager.GetUsersInRoleAsync(VaiTro.Role_Stu);
				result[VaiTro.Role_Stu] = usersInRole.Count;
			}

			ViewData["TeacherCount"] = result[VaiTro.Role_Teach];
			ViewData["StudentCount"] = result[VaiTro.Role_Stu];
			ViewData["UserCount"] = _db.Users.Count();

			ViewData["ExamCount"] = _db.DeThi.Count();

			ViewData["ActiveExam"] = _db.DeThi
										.Where(d => d.GioBD <= DateTime.Now && d.GioKT > DateTime.Now)
										.Count();

			ViewData["UpcomingExam"] = _db.DeThi
										  .Where(d => d.GioBD > DateTime.Now).Count();

			var thongBao = _db.ThongBaoAdmin
							.OrderByDescending(tb => tb.GioTB)
							.Take(6)
							.ToList();

			ViewData["Title"] = "Trang Quản trị";
			return View(thongBao);
		}
		public async Task<IActionResult> UserManager(int page = 1)
		{
			int pageSize = 5;
			var totalUsers = _userManager.Users.Count();


			var users = await _userManager.Users
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking().ToListAsync();

			ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
			ViewBag.CurrentPage = page;

			foreach (var u in users)
			{
				var roles = await _userManager.GetRolesAsync(u);
				u.Roles = roles.ToList();
			}
			return PartialView("_UserManager", users);
		}

		public IActionResult ExamManager()
		{
			var danhSachKyThi = _db.DeThi.AsNoTracking().ToList();
			return PartialView("_ExamManager", danhSachKyThi);
		}

		public IActionResult Post()
		{
			return PartialView("_Post");
		}

	}
}

