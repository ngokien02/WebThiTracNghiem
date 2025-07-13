using Newtonsoft.Json;

namespace WebThiTracNghiem.Models
{
	public class DapAn
	{
		public int Id { get; set; }
		public string NoiDung { get; set; }
		public Boolean DungSai { get; set; }
		public int CauHoiId { get; set; }
		[JsonIgnore]
		public virtual CauHoi CauHoi { get; set; }
	}
}