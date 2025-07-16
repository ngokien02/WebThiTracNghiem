using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebThiTracNghiem.Models;

[Route("Email/[action]")]
public class EmailController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public EmailController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> CheckExist(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return Json(new { exists = user != null });
    }
}