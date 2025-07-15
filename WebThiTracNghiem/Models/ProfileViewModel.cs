using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Models;

public class ProfileViewModel
{
    public string? HoTen { get; set; }
    public DateTime? NgaySinh { get; set; }
    public string? GioiTinh { get; set; }
    public string? CMND { get; set; }
    public string? DiaChi { get; set; }
    public string? LopHoc { get; set; }
    public string? Khoa { get; set; }
    public string? KhoaHoc { get; set; }
    public string? AvatarUrl { get; set; }
}
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
        {
            return NotFound("Không tìm thấy người dùng.");
        }

        return Page();
    }
}

