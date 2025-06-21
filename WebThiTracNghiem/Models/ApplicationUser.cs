using Microsoft.AspNetCore.Identity;

namespace WebThiTracNghiem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string MaSV { get; set; }
        public string HoTen { get; set; }
    }
}
