using Microsoft.AspNetCore.Identity;
using System.Runtime.CompilerServices;

namespace WebThiTracNghiem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? HoTen { get; set; }
		public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
		{
			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			var users = new List<(string username, string email, string password, string role)>
			{
				("Admin", "admin@admin", "Admin@123", SD.Role_Admin),
				("Teacher", "teacher@teacher", "Teacher@123", SD.Role_Teach),
				("Student", "student@student", "Student@123", SD.Role_Stu)
			};

			foreach (var (userName, email, password, role) in users)
			{
				if (await userManager.FindByEmailAsync(email) == null)
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
		}
	}
}
