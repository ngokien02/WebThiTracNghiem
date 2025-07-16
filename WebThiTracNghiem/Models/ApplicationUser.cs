using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebThiTracNghiem.Models
{
    public class ApplicationUser : IdentityUser
    {
		[Required]
		[MaxLength(10)]
		public string MaSV { get; set; }
		public string? HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? CMND { get; set; }
        public string? DiaChi { get; set; }
        public string? LopHoc { get; set; }
        public string? Khoa { get; set; }
        public string? KhoaHoc { get; set; }
        public string? AvatarUrl { get; set; }

        public static async Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Bước 1: Tạo các vai trò nếu chưa tồn tại
            string[] roles = { VaiTro.Role_Admin, VaiTro.Role_Teach, VaiTro.Role_Stu };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"[+] Đã tạo role: {roleName}");
                }
            }

            // Bước 2: Danh sách người dùng cần seed
            var users = new List<ApplicationUser>
    {
        new ApplicationUser { UserName = "Admin", Email = "admin@admin", PhoneNumber = "0900000001", HoTen = "Ngô Trung Hiếu Kiên", NgaySinh = new DateTime(1990, 1, 1), GioiTinh = "Nam", CMND = "123456789", DiaChi = "TP.HCM", LopHoc = "QT001", Khoa = "Hệ thống thông tin", KhoaHoc = "2020-2024", AvatarUrl = "/images/users/admin.jpg" },
        new ApplicationUser { UserName = "Teacher", Email = "teacher@teacher", PhoneNumber = "0900000002", HoTen = "Nguyễn Tấn Thành", NgaySinh = new DateTime(1985, 5, 20), GioiTinh = "Nữ", CMND = "987654321", DiaChi = "Hà Nội", LopHoc = "GV001", Khoa = "Công nghệ thông tin", KhoaHoc = "Giáo viên", AvatarUrl = "/images/users/teacher.jpg" },
        new ApplicationUser { UserName = "Student", Email = "student@student", PhoneNumber = "0900000003", HoTen = "Lê Hoàng Hiếu", NgaySinh = new DateTime(2002, 9, 10), GioiTinh = "Nam", CMND = "111222333", DiaChi = "Đà Nẵng", LopHoc = "DHTT16A", Khoa = "Khoa học máy tính", KhoaHoc = "2021-2025", AvatarUrl = "/images/users/student.jpg" },
        new ApplicationUser { UserName = "DuyKhanh", Email = "kxtduykhanh@gmail.com", PhoneNumber = "094343352", HoTen = "Đặng Nguyễn Duy Khanh", NgaySinh = new DateTime(2003, 4, 15), GioiTinh = "Nam", CMND = "999888777", DiaChi = "Cần Thơ", LopHoc = "DHTT16B", Khoa = "Công nghệ phần mềm", KhoaHoc = "2021-2025", AvatarUrl = "/images/users/duykhanh.jpg" }
    };

            // Bước 3: Tạo người dùng nếu chưa tồn tại
            foreach (var user in users)
            {
                if (await userManager.FindByEmailAsync(user.Email) == null)
                {
                    string password = user.UserName switch
                    {
                        "Admin" => "Admin@123",
                        "Teacher" => "Teacher@123",
                        "Student" => "Student@123",
                        "DuyKhanh" => "Admin@123",
                        _ => "Default@123"
                    };

                    string role = user.UserName switch
                    {
                        "Admin" => VaiTro.Role_Admin,
                        "Teacher" => VaiTro.Role_Teach,
                        _ => VaiTro.Role_Stu
                    };

                    user.EmailConfirmed = true;

                    var result = await userManager.CreateAsync(user, password);
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                        Console.WriteLine($"[✔] Tạo user '{user.UserName}' và gán role '{role}' thành công.");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"[!] Tạo user '{user.UserName}' thất bại: {error.Description}");
                        }
                    }
                }
            }
        }
    }
    }
