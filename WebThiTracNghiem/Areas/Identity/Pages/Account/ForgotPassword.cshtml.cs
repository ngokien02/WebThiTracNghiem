// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using WebThiTracNghiem.Models;

namespace WebThiTracNghiem.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordModel> _logger;
        private readonly IMemoryCache _cache;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<ForgotPasswordModel> logger,
            IMemoryCache cache)

        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _cache = cache;
        }


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
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            // Kiểm tra xem có bị giới hạn gửi email hay không
            var cacheKey = $"ResetEmail_{Input.Email}";

            if (_cache.TryGetValue(cacheKey, out _))
            {
                ModelState.AddModelError(string.Empty, "Bạn vừa yêu cầu gửi email, vui lòng thử lại sau 1 phút.");
                return Page();
            }

            // Nếu chưa bị giới hạn → lưu vào cache trong 60 giây
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(60));


            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy email của bạn trong hệ thống.");
                return Page();
            }
            bool emailExists = user != null;
            bool emailConfirmed = emailExists && await _userManager.IsEmailConfirmedAsync(user);

            // Ghi log để bạn dễ test nội bộ (nếu dùng Email giả)
            _logger.LogInformation("🧪 Kiểm tra Email: {Email} → Tồn tại: {Exists}, Đã xác nhận: {Confirmed}",
                Input.Email, emailExists, emailConfirmed);

            // ✅ Nếu không tồn tại hoặc chưa xác nhận → vẫn chuyển đến trang xác nhận để tránh lộ info
            if (!emailExists || !emailConfirmed)
            {
                TempData["ResetPasswordMessage"] = "Nếu email tồn tại và đã xác nhận, liên kết đặt lại mật khẩu sẽ được gửi.";
                return RedirectToPage("./ForgotPasswordConfirmation");
            }


            // Tạo mã đặt lại mật khẩu
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Tạo đường dẫn reset password
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", code = encodedCode },
                protocol: Request.Scheme);

            // ✅ Log ra Output (Visual Studio → Output → Debug)
            System.Diagnostics.Debug.WriteLine("🔗 LINK TEST: " + callbackUrl);

            // ✅ Hiển thị trong Razor view (nếu bạn gắn ở ForgotPassword.cshtml)
            ViewData["DebugResetLink"] = callbackUrl;

            // Gửi email (nếu dùng thật)
            var htmlMessage = $@"
  <div style='font-family:Segoe UI,Helvetica,sans-serif;max-width:600px;margin:auto;border:1px solid #ddd;border-radius:10px;padding:30px;background:#1e1e1e;color:#fff'>
<!-- Logo GTQuiz -->
    <div style='text-align:center; margin-bottom:20px'>
      <img src='/static/media/banner_header.cabeb951.png' alt='CD-GTVT.Logo' style='height:60px;' />
    </div>
    <h2 style='color:#ffd700;margin-bottom:20px;'>🔐 Yêu cầu đặt lại mật khẩu</h2>
    <p style='font-size:1rem;'>Xin chào <strong style='color:#4dabf7'>{user.UserName}</strong>,</p>
    <p style='margin:16px 0;'>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản thi trắc nghiệm của trường Cao đẳng Giao Thông Vận Tải của bạn.</p>

    <div style='text-align:center;margin:30px 0'>
      <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='background-color:#0d6efd;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold;display:inline-block;font-size:16px'>
        Đặt lại mật khẩu
      </a>
    </div>

    <p style='font-size:0.9rem;color:#ccc;'>
      Nếu bạn không yêu cầu điều này, bạn có thể bỏ qua email này. Mật khẩu của bạn sẽ không thay đổi trừ khi bạn truy cập vào liên kết ở trên và tạo mật khẩu mới.
    </p>

    <hr style='margin:30px 0;border:0;border-top:1px solid #444' />

    <p style='font-size:0.8rem;color:#888;text-align:center'>
      🛡️ Đây là email tự động. Vui lòng không trả lời.<br />
      © {DateTime.Now.Year} GTQuiz. All rights reserved.
    </p>
  </div>";


            await _emailSender.SendEmailAsync(
                Input.Email,
                "🔐 Yêu cầu đặt lại mật khẩu - Cao đẳng giao thông vận tải",
                htmlMessage);


            // ✅ Gán thông báo vào TempData
            TempData["ResetPasswordMessage"] = "Vui lòng kiểm tra email để đặt lại mật khẩu.";
            // Ghi log thông tin
            _logger.LogInformation("🔁 Người dùng gửi yêu cầu quên mật khẩu: {Email}", Input.Email);

            try
            {
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    htmlMessage
                );
                _logger.LogInformation("📧 Gửi email đặt lại mật khẩu thành công cho {Email}", Input.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi gửi email cho {Email}", Input.Email);
            }
            // Chuyển hướng đến trang xác nhận
            return RedirectToPage("./ForgotPasswordConfirmation");


        }
    }
}