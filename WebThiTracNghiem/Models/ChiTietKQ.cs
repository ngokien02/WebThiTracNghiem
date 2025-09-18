using System.ComponentModel.DataAnnotations.Schema;

namespace WebThiTracNghiem.Models
{
    public class ChiTietKQ
    {
        public int Id { get; set; }

        public int KetQuaId { get; set; }
        [ForeignKey("KetQuaId")]
        public virtual KetQua KetQua { get; set; }

        public int CauHoiId { get; set; }
        [ForeignKey("CauHoiId")]
        public virtual CauHoi CauHoi { get; set; }

        public double? Diem { get; set; }

        public virtual ICollection<ChiTietKQ_DapAn> DapAnChons { get; set; }
    }
}
