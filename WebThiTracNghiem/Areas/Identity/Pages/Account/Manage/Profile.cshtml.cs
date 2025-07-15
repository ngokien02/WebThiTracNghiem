using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Identity.Pages.Account.Manage
{
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public ApplicationUser UserInfo { get; set; }

		public async Task<IActionResult> OnGetAsync()
		{
			UserInfo = await _userManager.GetUserAsync(User);
			if (UserInfo == null)
				return NotFound();

			// Kiểm tra dữ liệu lấy được
			Console.WriteLine($"[DEBUG] Đăng nhập: {UserInfo.UserName} - {UserInfo.HoTen}");
			var idFromClaims = _userManager.GetUserId(User);
			var userFromManager = await _userManager.GetUserAsync(User);
			Console.WriteLine($"[DEBUG] ClaimsId: {idFromClaims}");
			Console.WriteLine($"[DEBUG] UserName: {userFromManager?.UserName}");
			Console.WriteLine($"[DEBUG] HoTen: {userFromManager?.HoTen}");


			return Page();
		}


	}
}
