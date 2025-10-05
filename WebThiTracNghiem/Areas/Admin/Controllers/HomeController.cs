using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
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
		public async Task<IActionResult> UserManager(int page = 1, string keyword = "", string role = "", string status = "")
		{
			try
			{
				int pageSize = 5;
				var query = _userManager.Users.AsQueryable();

				// 🔍 Lọc theo keyword
				if (!string.IsNullOrWhiteSpace(keyword))
				{
					keyword = keyword.ToLower();
					query = query.Where(u =>
						(u.HoTen != null && u.HoTen.ToLower().Contains(keyword)) ||
						(u.Email != null && u.Email.ToLower().Contains(keyword)) ||
						(u.UserName != null && u.UserName.ToLower().Contains(keyword))
					);
				}

				// ⚙️ Lọc role
				if (!string.IsNullOrEmpty(role))
				{
					var usersInRole = await _userManager.GetUsersInRoleAsync(role);
					var userIds = usersInRole.Select(u => u.Id).ToList();

					query = query.Where(u => userIds.Contains(u.Id));
				}

				// ⚙️ Lọc trạng thái
				if (!string.IsNullOrEmpty(status))
				{
					if (status == "active")
						query = query.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow);
					else if (status == "inactive")
						query = query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow);
				}

				// Phân trang
				var totalUsers = await query.CountAsync();
				var users = await query
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.AsNoTracking()
					.ToListAsync();

				// ⚠️ Chỗ dễ lỗi NullReference
				foreach (var u in users)
				{
					u.Roles = (await _userManager.GetRolesAsync(u)).ToList();
				}

				ViewBag.TotalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
				ViewBag.CurrentPage = page;
				ViewBag.FromPage = Math.Max(1, page - 2);
				ViewBag.ToPage = Math.Min(ViewBag.TotalPages, page + 2);

				return PartialView("_UserManager", users);
			}
			catch (Exception ex)
			{
				// Ghi log ra console
				Console.WriteLine("❌ ERROR in UserManager: " + ex.Message);
				Console.WriteLine(ex.StackTrace);

				return StatusCode(500, "Internal server error: " + ex.Message);
			}
		}


		public async Task<IActionResult> ExamManager(int page = 1, string keyword = "", string status = "")
		{
			int pageSize = 5; // số lượng exam mỗi trang
			var query = _db.DeThi.AsQueryable();

			// 🔍 Lọc theo keyword
			if (!string.IsNullOrWhiteSpace(keyword))
			{
				keyword = keyword.ToLower();
				query = query.Where(d =>
		 (d.TieuDe != null && d.TieuDe.ToLower().Contains(keyword)) ||
		 (d.MaDe != null && d.MaDe.ToLower().Contains(keyword))
	 );
			}
			var now = DateTime.Now;
			if (!string.IsNullOrEmpty(status))
			{
				if (status == "running")
					query = query.Where(d => d.GioBD <= now && d.GioKT > now);
				else if (status == "upcoming")
					query = query.Where(d => d.GioBD > now);
				else if (status == "finished")
					query = query.Where(d => d.GioKT <= now);
			}

			// Phân trang
			var totalExams = await query.CountAsync();
			var exams = await query
				.OrderBy(d => d.Id)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync();

			// Truyền thông tin phân trang xuống view
			ViewBag.TotalPages = (int)Math.Ceiling((double)totalExams / pageSize);
			ViewBag.CurrentPage = page;
			ViewBag.Keyword = keyword;
			ViewBag.Status = status;
			ViewBag.FromPage = Math.Max(1, ViewBag.CurrentPage - 2);
			ViewBag.ToPage = Math.Min(ViewBag.TotalPages, ViewBag.CurrentPage + 2);
			return PartialView("_ExamManager", exams);
		}

		public IActionResult Guide()
		{
			ViewData["Title"] = "Trang hướng dẫn";
			return PartialView("_Guide");
		}
	}
}

