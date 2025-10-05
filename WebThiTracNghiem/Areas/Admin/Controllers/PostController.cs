using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebThiTracNghiem.Areas.Admin.Models;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class PostController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IWebHostEnvironment _hosting;

		public PostController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext db)
		{
			_db = db;
			_hosting = webHostEnvironment;
		}

		public IActionResult Index()
		{
			var posts = _db.Post
				.Include(p => p.User)
				.Include(p => p.DeMucs)
				.OrderByDescending(p => p.ThoiGian)
				.ToList();

			return PartialView("Index", posts);
		}

		[HttpGet]
		public IActionResult CreatePost()
		{
			return PartialView("_CreatePost");
		}

		[HttpPost]
		public async Task<IActionResult> CreatePost(Post post)
		{

			// ✅ Gán UserId (từ Identity)
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			post.UserId = userId;
			post.ThoiGian = DateTime.Now;

			// ✅ Xử lý các đề mục (DeMuc)
			foreach (var demuc in post.DeMucs)
			{
				if (demuc.ImageFile != null)
				{
					demuc.ImageUrl = SaveImage(demuc.ImageFile);
				}
				demuc.Post = post;
			}

			_db.Post.Add(post);
			await _db.SaveChangesAsync();

			return Ok(new { success = true, message = "Thêm bài viết thành công", postId = post.Id });
		}

		private string SaveImage(IFormFile img)
		{
			var fileName = Guid.NewGuid().ToString() + Path.GetExtension(img.FileName);
			var path = Path.Combine(_hosting.WebRootPath, "images", "post");
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			var saveFile = Path.Combine(path, fileName);
			using (var stream = new FileStream(saveFile, FileMode.Create))
			{
				img.CopyTo(stream);
			}

			return fileName;
		}

		public async Task<IActionResult> getPostById(int id)
		{
			var post = await _db.Post
				.Include(p => p.User)
				.Include(p => p.DeMucs)
				.FirstOrDefaultAsync(p => p.Id == id);

			if (post == null)
			{
				return NotFound();
			}

			return PartialView("_PostById", post);
		}
	}
}
