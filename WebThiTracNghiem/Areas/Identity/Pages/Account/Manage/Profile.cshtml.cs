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

            return Page();
        }
    }
}