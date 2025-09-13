using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebThiTracNghiem.Models
{
	public class CauHoi
	{
		public int Id { get; set; }
		public string NoiDung { get; set; }
		public string Loai {  get; set; }
        public int ChuDeId { get; set; }
		[ForeignKey("ChuDeId")]
        public ChuDe ChuDe { get; set; }
        public List<DapAn> DapAnList { get; set; } = new();
    }
}
