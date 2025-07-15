using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Controllers
{
	public class ProfileController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public ProfileController(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			return View(user);
		}

		[HttpPost]
		public async Task<IActionResult> Update([FromBody] ApplicationUser model)
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();

			// Cập nhật tất cả các trường cần thiết
			user.HoTen = model.HoTen;
			user.PhoneNumber = model.PhoneNumber;
			user.NgaySinh = model.NgaySinh;
			user.GioiTinh = model.GioiTinh;
			user.CMND = model.CMND;
			user.DiaChi = model.DiaChi;
			user.Email = model.Email; // Nếu cho phép chỉnh email
			user.AvatarUrl = model.AvatarUrl;
			user.Khoa = model.Khoa;
			user.LopHoc = model.LopHoc;
			user.KhoaHoc = model.KhoaHoc;

			// Bạn có thể thêm các trường bảo mật nếu được phép sửa như mật khẩu (cần xử lý riêng)

			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
				return Json(new { success = true });
			else
				return BadRequest(result.Errors);
		}
		[HttpPost]
        [Route("Profile/ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
		{
			// Kiểm tra dữ liệu đầu vào
			if (string.IsNullOrWhiteSpace(model.CurrentPassword) || string.IsNullOrWhiteSpace(model.NewPassword))
			{
				return BadRequest(new
				{
					errors = new[] { new { description = "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới." } }
				});
			}

			if (string.IsNullOrWhiteSpace(model.NewPassword) ||
				model.NewPassword != model.ConfirmPassword)
			{
				return BadRequest(new { errors = new[] { new { description = "Mật khẩu mới không khớp hoặc trống." } } });
			}

			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            foreach (var error in result.Errors)
            {
                Console.WriteLine("❌ ChangePassword Error: " + error.Description);
            }

            if (result.Succeeded)
				return Ok(new { success = true });
			else
				return BadRequest(new { errors = result.Errors });

		}

	}
}