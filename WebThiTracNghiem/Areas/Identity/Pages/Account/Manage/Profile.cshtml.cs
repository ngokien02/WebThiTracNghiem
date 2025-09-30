using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        public async Task<JsonResult> OnGetCheckExistAsync(string type, string value)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(value))
                return new JsonResult(false);

            bool exists = false;

            var user = await _userManager.GetUserAsync(User);
            switch (type.ToLower())
            {
                case "email":
                    var existingEmail = await _userManager.FindByEmailAsync(value);
                    exists = existingEmail != null && existingEmail.Id != user.Id;
                    break;
                case "cmnd":
                    exists = await _userManager.Users.AnyAsync(u => u.CCCD == value && u.Id != user.Id);
                    break;
                case "phonenumber":
                    var existingPhone = await _userManager.Users.AnyAsync(u => u.PhoneNumber == value && u.Id != user.Id);
                    exists = existingPhone;
                    break;
            }

            return new JsonResult(exists);
        }
    }
}