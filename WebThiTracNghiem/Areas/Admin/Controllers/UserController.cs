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
		private readonly RoleManager<IdentityRole> _roleManager;

		public UserController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_db = db;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		public async Task<IActionResult> CreateUser(string role, string username, string hoTen, string password)
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

		[HttpPost]
		public async Task<IActionResult> ImportUsers([FromBody] List<Dictionary<string, string>> users)
		{
			if (users == null || !users.Any())
				return BadRequest("Không có dữ liệu.");

			if (!await _roleManager.RoleExistsAsync("student"))
			{
				await _roleManager.CreateAsync(new IdentityRole("student"));
			}

			var errors = new List<string>();

			foreach (var u in users)
			{
				var username = u.ContainsKey("username") ? u["username"] : null;
				var password = u.ContainsKey("password") ? u["password"] : null;

				if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
				{
					errors.Add($"Dữ liệu không hợp lệ cho user {username}");
					continue;
				}

				var existingUser = await _userManager.FindByNameAsync(username);
				if (existingUser != null)
				{
					errors.Add($"User {username} đã tồn tại.");
					continue;
				}

				var newUser = new ApplicationUser
				{
					UserName = username,
					Email = $"{username}@example.com"
				};

				var result = await _userManager.CreateAsync(newUser, password);

				if (result.Succeeded)
				{
					await _userManager.AddToRoleAsync(newUser, "student");
				}
				else
				{
					errors.Add($"Không tạo được user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
				}
			}

			if (errors.Any())
				return BadRequest(new { message = "Có lỗi khi import", details = errors });

			return Ok(new { message = "Import thành công tất cả user" });
		}

	}
}
