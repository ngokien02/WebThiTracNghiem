using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebThiTracNghiem.Areas.Admin.Models;

namespace WebThiTracNghiem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<DeThi> DeThi { get; set; }
        public DbSet<CauHoi> CauHoi { get; set; }
        public DbSet<DapAn> DapAn { get; set; }
        public DbSet<KetQua> KetQua { get; set; }
        public DbSet<AdminNotification> ThongBaoAdmin { get; set; }
	}
}
