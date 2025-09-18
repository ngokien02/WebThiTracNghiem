using System.ComponentModel.DataAnnotations.Schema;

namespace WebThiTracNghiem.Models
{
    public class ChiTietKQ_DapAn
    {
        public int Id { get; set; }

        public int ChiTietKQId { get; set; }
        [ForeignKey("ChiTietKQId")]
        public virtual ChiTietKQ ChiTietKQ { get; set; }

        public int DapAnId { get; set; }
        [ForeignKey("DapAnId")]
        public virtual DapAn DapAn { get; set; }
    }
}
