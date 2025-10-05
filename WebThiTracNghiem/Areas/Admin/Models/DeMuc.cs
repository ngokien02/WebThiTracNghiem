using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebThiTracNghiem.Areas.Admin.Models
{
	public class DeMuc
	{
		[Key]
		public int Id { get; set; }

		public int PostId { get; set; }

		[ForeignKey("PostId")]
		public virtual Post Post { get; set; }

		public string TieuDe { get; set; }

		public string NoiDung { get; set; }
		
		public string? ImageUrl { get; set; }

		[NotMapped]
		public IFormFile? ImageFile {  get; set; } 

		public int STT { get; set; } 
	}
}