using Microsoft.AspNetCore.Identity;

namespace WebThiTracNghiem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? HoTen { get; set; }
		public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
		{
			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

			var user = await userManager.FindByNameAsync("student");
			if (user == null)
			{
				var newUser = new ApplicationUser
				{
					UserName = "student",
					Email = "student@student.com",
					EmailConfirmed = true 
				};

				await userManager.CreateAsync(newUser, "Student@123"); 
				await userManager.AddToRoleAsync(newUser, SD.Role_Stu);
			}
		}
	}
}
