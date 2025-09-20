using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Runtime.CompilerServices;

namespace WebThiTracNghiem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? DiaChi { get; set; }
        public string? LopHoc { get; set; }
        public string? Khoa { get; set; }
        public string? KhoaHoc { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CCCD { get; set; }
        [NotMapped]
        public IFormFile? avtImg { get; set; }
		[NotMapped]
		public List<string> Roles { get; set; } = new();
		public virtual ICollection<KetQua> KetQuaList { get; set; }
            public static async Task SeedUserAsync(IServiceProvider serviceProvider)
            {
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var roles = new[] { VaiTro.Role_Admin, VaiTro.Role_Teach, VaiTro.Role_Stu };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
                var users = new List<(string username, string email, string password, string role)>
                {
                    ("Admin", "ngokien982@gmail.com", "Admin@123", VaiTro.Role_Admin),
                    ("Teacher", "teacher@teacher", "Teacher@123", VaiTro.Role_Teach),
                    ("Student", "student@student", "Student@123", VaiTro.Role_Stu)
                };


                foreach (var (userName, email, password, role) in users)
                {
                    var existingUsers = userManager.Users.Where(u => u.Email == email).ToList();
                    if (!existingUsers.Any())
                    {
                        var user = new ApplicationUser
                        {
                            UserName = userName,
                            Email = email,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, role);
                        }
                    }
                }

                for (int i = 1; i <= 150; i++)
                {
                    string username = $"TH{i:D3}"; 
                    string password = $"{username}K20toi@123";

                    var existingUser = await userManager.FindByNameAsync(username);
                    if (existingUser == null)
                    {
                        var user = new ApplicationUser
                        {
                            UserName = username,
                            Email = null,
                            EmailConfirmed = true
                        };

                        var result = await userManager.CreateAsync(user, password);
                        if (result.Succeeded)
                        {
                            await userManager.AddToRoleAsync(user, VaiTro.Role_Stu);
                        }
                    }
                }
            }
    }
}