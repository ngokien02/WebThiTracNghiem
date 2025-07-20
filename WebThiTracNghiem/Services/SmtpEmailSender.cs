using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace WebThiTracNghiem.Services
{
	public class SmtpEmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;

		public SmtpEmailSender(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var message = new MimeMessage();
			message.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:From"]));
			message.To.Add(MailboxAddress.Parse(email));
			message.Subject = subject;

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = htmlMessage
			};
			message.Body = bodyBuilder.ToMessageBody();

			using var client = new SmtpClient();
			await client.ConnectAsync(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:Port"]), true);
			await client.AuthenticateAsync(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);
			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}
	}
}