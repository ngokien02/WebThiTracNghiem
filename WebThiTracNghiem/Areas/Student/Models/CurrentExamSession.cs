namespace WebThiTracNghiem.Areas.Student.Models
{
	public class CurrentExamSession
	{
		public DeThiViewModel DeThi { get; set; }
		public DateTime TimeStart { get; set; }
		public bool IsDone { get; set; }
	}
}
