// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebThiTracNghiem.Migrations;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            public string Id { get; set; }

            [Display(Name = "Họ tên")]
            public string HoTen { get; set; }

            [Display(Name = "Ngày sinh")]
            [DataType(DataType.Date)]
            public DateTime? NgaySinh { get; set; }

            [Display(Name = "Lớp học")]
            public string LopHoc { get; set; }

            [Display(Name = "Ảnh đại diện (URL)")]
            public string AvatarUrl { get; set; }

            [EmailAddress]
            public string Email { get; set; }

            [Phone]
            public string PhoneNumber { get; set; }

            public string Username { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            Input = new InputModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                HoTen = user.HoTen,
                NgaySinh = user.NgaySinh,
                LopHoc = user.LopHoc,
                AvatarUrl = user.AvatarUrl
            };
        }


        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Cập nhật
            user.HoTen = Input.HoTen;
            user.NgaySinh = Input.NgaySinh;
            user.LopHoc = Input.LopHoc;
            user.AvatarUrl = Input.AvatarUrl;
            user.PhoneNumber = Input.PhoneNumber;
            user.Email = Input.Email;


            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "✅ Hồ sơ đã được cập nhật.";
            return RedirectToPage();
        }
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user); // ✅ BẮT BUỘC PHẢI GỌI DÒNG NÀY

            return Page();
        }
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return new JsonResult(new { success = false, message = "Không tìm thấy người dùng" });

            user.HoTen = Input.HoTen;
            user.NgaySinh = Input.NgaySinh;
            user.LopHoc = Input.LopHoc;
            user.PhoneNumber = Input.PhoneNumber;
            user.AvatarUrl = Input.AvatarUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return new JsonResult(new { success = false, message = "Lỗi khi cập nhật" });

            await _signInManager.RefreshSignInAsync(user);
            return new JsonResult(new { success = true, message = "Thông tin đã được cập nhật!" });
        }

    }
}
