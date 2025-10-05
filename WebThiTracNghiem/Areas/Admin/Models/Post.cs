using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Models
{
	public class Post
	{
		[Key]
		public int Id { get; set; }

		public string TieuDe { get; set; }

		public string? TomTat { get; set; } 

		public DateTime ThoiGian { get; set; }

		[ForeignKey("User")]
		public string UserId { get; set; }
		public virtual ApplicationUser User { get; set; }

		public virtual ICollection<DeMuc> DeMucs { get; set; } = new List<DeMuc>();
	}
}
