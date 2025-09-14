using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThiTracNghiem.Models
{
        public class DeThi
        {
		    [Key]
		    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		    public int Id { get; set; }
            public string TieuDe {  get; set; }
            public string MaDe { get; set; }
            public DateTime GioBD { get; set; }
            public DateTime GioKT { get; set; }
            public int ThoiGian { get; set; }
            public int SoCauHoi { get; set; }
            public double DiemToiDa { get; set; }
            public Boolean RandomCauHoi { get; set; }
            public Boolean RandomDapAn {  get; set; }
            public Boolean ShowKQ { get; set; }
            public string IdGiangVien {  get; set; }
            [ForeignKey("IdGiangVien")]
            public virtual ApplicationUser GiangVien { get; set; }
            public virtual ICollection<KetQua> KetQuaList { get; set; }
            public virtual ICollection<ChiTietDeThi> ChiTietDe { get; set; }
        }
}
