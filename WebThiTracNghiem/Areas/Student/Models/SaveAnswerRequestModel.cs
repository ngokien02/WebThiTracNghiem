namespace WebThiTracNghiem.Areas.Student.Models
{
	public class SaveAnswerRequestModel
	{
		public int CauHoiId { get; set; }
		public List<int> DapAnIds { get; set; }
	}
}
