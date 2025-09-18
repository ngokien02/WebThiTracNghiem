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
        public DbSet<ChiTietDeThi> ChiTietDeThi { get; set; }
        public DbSet<ChiTietKQ> ChiTietKQ { get; set; }
        public DbSet<ChiTietKQ_DapAn> ChiTietKQ_DapAn { get; set; }
        public DbSet<ChuDe> ChuDe { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ChiTietKQ_DapAn>()
                .HasOne(ct => ct.DapAn)
                .WithMany()
                .HasForeignKey(ct => ct.DapAnId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<ChiTietKQ_DapAn>()
                .HasOne(ct => ct.ChiTietKQ)
                .WithMany(ct => ct.DapAnChons)
                .HasForeignKey(ct => ct.ChiTietKQId)
                .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
