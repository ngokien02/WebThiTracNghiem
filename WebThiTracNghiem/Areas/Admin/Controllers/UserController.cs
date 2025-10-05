using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Areas.Admin.Models;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
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

		[HttpGet]
		public async Task<IActionResult> getEditUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return Json(new
				{
					success = false,
					message = "Không tìm thấy người dùng này"
				});
			}

			var role = await _userManager.GetRolesAsync(user);

			return Json(new
			{
				success = true,
				id = user.Id,
				username = user.UserName,
				hoTen = user.HoTen,
				role
			});
		}

		[HttpPost]
		public async Task<IActionResult> editUser(IFormCollection fd)
		{
			string id = fd["id"];
			string username = fd["username"];
			string hoTen = fd["hoTen"];
			string password = fd["password"];
			string role = fd["role"];

			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return Json(new
				{
					success = false,
					message = "Không tìm thấy người dùng này!"
				});
			}

			user.UserName = fd["username"].ToString();
			user.HoTen = fd["hoTen"].ToString();

			if (!string.IsNullOrEmpty(password))
			{
				var token = await _userManager.GeneratePasswordResetTokenAsync(user);
				var resetPassResult = await _userManager.ResetPasswordAsync(user, token, password);
				if (!resetPassResult.Succeeded)
				{
					return Json(new
					{
						success = false,
						message = "Lỗi không đổi được mật khẩu"
					});
				}
			}

			var updateResult = await _userManager.UpdateAsync(user);

			if (!updateResult.Succeeded)
			{
				return Json(new
				{
					success = false,
					message = "Lỗi khi cập nhật, vui lòng tải lại trang!"
				});
			}

			var currentRole = await _userManager.GetRolesAsync(user);

            if (currentRole.Any())
            {
				await _userManager.RemoveFromRolesAsync(user, currentRole);
            }

			if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role)) {
				await _userManager.AddToRoleAsync(user, role);
			}

			return Json(new
			{
				success = true,
				message = "Cập nhật thành công!"
			});
        }

		[HttpPost]
		public async Task<IActionResult> deleteUser(string id)
		{
			var user = await _userManager.FindByIdAsync(id);

			if (user == null)
			{
				return Json(new
				{
					success = false,
					message = "Không tìm thấy người dùng này"
				});
			}

			await _userManager.DeleteAsync(user);

			return Json(new
			{
				success = true
			});
		}

		[HttpGet]
		public async Task<IActionResult> GetAllUsers(string? keyword, string? role, string? status)
		{
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

			// ⚙️ Lọc theo vai trò
			if (!string.IsNullOrEmpty(role))
			{
				var usersInRole = await _userManager.GetUsersInRoleAsync(role);
				var userIds = usersInRole.Select(u => u.Id);
				query = query.Where(u => userIds.Contains(u.Id));
			}

			// ⚙️ Lọc theo trạng thái (đồng nhất với UserManager)
			if (!string.IsNullOrEmpty(status))
			{
				if (status == "active")
					query = query.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTimeOffset.UtcNow);
				else if (status == "inactive")
					query = query.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow);
			}

			// ✅ Lấy toàn bộ user (không phân trang)
			var users = await query.AsNoTracking().ToListAsync();

			var result = new List<object>();
			foreach (var u in users)
			{
				var roles = await _userManager.GetRolesAsync(u);
				result.Add(new
				{
					UserName = u.UserName,
					HoTen = u.HoTen,
					Email = u.Email,
					VaiTro = string.Join(", ", roles),
					TrangThai = (u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow) ? "Khóa" : "Hoạt động"
				});
			}

			return Json(result);
		}
	}
}
