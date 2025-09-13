using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Areas.Admin.Models;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class UserController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly UserManager<ApplicationUser> _userManager;

		public UserController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_userManager = userManager;
		}
		public async Task<IActionResult> CreateUser(string role, string username, string hoTen,	string password)
		{
			var existingUser = await _userManager.FindByNameAsync(username);
			if (existingUser != null)
			{
				return Json(new { success = false, message = "Tên đăng nhập đã tồn tại!" });
			}

			var user = new ApplicationUser
			{
				UserName = username,
				HoTen = hoTen,
			};

			var result = await _userManager.CreateAsync(user, password);

			if (!result.Succeeded)
			{
				return Json(new
				{
					success = false,
					message = string.Join(",", result.Errors.Select(e => e.Description))
				});
			}

			await _userManager.AddToRoleAsync(user, role);

			//tao thong bao admin
			var tbAdmin = new AdminNotification
			{
				LoaiTB = "TaoUser",
				TieuDe = user.UserName,
				GioTB = DateTime.Now
			};

			_db.ThongBaoAdmin.Add(tbAdmin);
			_db.SaveChanges();

			return Json(new
			{
				success = true,
				message = "Tạo người dùng mới thành công!"
			});
		}
	}
}
