namespace WebThiTracNghiem.Areas.Student.Models
{
	public class CauHoiViewModel
	{
		public int Id { get; set; }
		public string NoiDung { get; set; }
		public string Loai { get; set; }

		public List<DapAnViewModel> DapAnList { get; set; }
	}
}
