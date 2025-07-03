namespace WebThiTracNghiem.Models
{
	public class CauHoi
	{
		public int Id { get; set; }
		public string NoiDung { get; set; }
		public string Loai {  get; set; }
		public string DeThiId { get; set; }
		public virtual DeThi DeThi { get; set; }
		public List<DapAn> DapAnList { get; set; } = new();

	}
}
