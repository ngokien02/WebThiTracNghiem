namespace WebThiTracNghiem.Areas.Student.Models
{
	public class DeThiViewModel
	{
		public int Id { get; set; }
		public string TieuDe { get; set; }
		public string MaDe { get; set; }

		public DateTime GioBD { get; set; }
		public DateTime GioKT { get; set; }

		public int ThoiGian { get; set; }
		public int SoCauHoi { get; set; }

		public double DiemToiDa { get; set; }
		public Boolean RandomCauHoi { get; set; }
		public Boolean RandomDapAn { get; set; }
		public Boolean ShowKQ { get; set; }

		public List<CauHoiViewModel> CauHoiList { get; set; }
	}
}
