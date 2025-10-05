using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using WebThiTracNghiem.Models;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace WebThiTracNghiem.Controllers
{
	public class ProfileController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hosting;

		public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IWebHostEnvironment environment)
		{
			_userManager = userManager;
			_db = dbContext;
            _hosting = environment;
		}

		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null) return NotFound();
			var role = User.IsInRole("Teacher") ? "Teacher"
		 : User.IsInRole("Admin") ? "Admin"
		 : "Student";
			ViewBag.RoleName = role;
			Console.WriteLine("✅ Role hiện tại: " + role);
			return View(user);
		}

        [HttpPost]
        public async Task<IActionResult> Update(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Kiểm tra trùng CMND nếu có
            if (!string.IsNullOrWhiteSpace(model.CCCD))
            {
                bool cmndExist = _db.Users.Any(u => u.CCCD == model.CCCD && u.Id != user.Id);
                if (cmndExist)
                    return BadRequest("❌ CCCD đã tồn tại trong hệ thống.");
            }

            // Cập nhật tất cả các trường cần thiết
            user.HoTen = model.HoTen;
            user.PhoneNumber = model.PhoneNumber;

            if (model.NgaySinh.HasValue &&
                DateTime.TryParseExact(model.NgaySinh.Value.ToString("dd/MM/yyyy"), "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var ngaySinh))
            {
                user.NgaySinh = ngaySinh;
            }
            else
            {
                return BadRequest("Ngày sinh sai định dạng.");
            }

            user.GioiTinh = model.GioiTinh;
            user.CCCD = model.CCCD;
            user.DiaChi = model.DiaChi;
            user.Email = model.Email;

            if (model.avtImg != null)
            {
                user.AvatarUrl = SaveImage(model.avtImg);
            }

            user.Khoa = model.Khoa;
            user.LopHoc = model.LopHoc;
            user.KhoaHoc = model.KhoaHoc;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return Json(new { success = true });
            else
                return BadRequest(result.Errors);
        }

        private string SaveImage(IFormFile img)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
            var path = Path.Combine(_hosting.WebRootPath, @"images/avatar/");
            var saveFile = Path.Combine(path, fileName);
            using (var filestream = new FileStream(saveFile, FileMode.Create))
            {
                img.CopyTo(filestream);
            }
            return fileName;
        }

        [HttpPost]
		[Route("Profile/ChangePassword")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordViewModel model)
		{
			// ✅ Bắt đầu bằng kiểm tra null
			if (model == null)
				return BadRequest(new { error = "Thiếu dữ liệu từ client." });
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
			if (model == null)
			{
				return BadRequest(new { error = "Thiếu dữ liệu từ client." });
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