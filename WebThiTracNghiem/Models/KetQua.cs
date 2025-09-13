using System.ComponentModel.DataAnnotations.Schema;

namespace WebThiTracNghiem.Models
{
	public class KetQua
	{
		public int Id { get; set; }
		public string IdSinhVien { get; set; }

		[ForeignKey("IdSinhVien")]
		public virtual ApplicationUser SinhVien { get; set; }
		public int DeThiId { get; set; }
		[ForeignKey("DeThiId")]
		public virtual DeThi DeThi { get; set; }
		public int? SoCauDung { get; set; }
		public double? Diem { get; set; }
		public DateTime GioLam { get; set; }
		public DateTime? GioNop { get; set; }
		public string TrangThai { get; set; }
        public virtual ICollection<ChiTietKQ> ChiTietKQs { get; set; }
    }
}
