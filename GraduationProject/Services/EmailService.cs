using System.Net.Mail;
using System.Net;

public interface IEmailService
{
	Task SendEmailAsync(string toEmail, string subject, string body);
}

public class EmailService : IEmailService
{
	private readonly IConfiguration _configuration;

	public EmailService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string body)
	{
		var smtpServer = _configuration["EmailSettings:SmtpServer"];
		var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
		var senderEmail = _configuration["EmailSettings:SenderEmail"];
		var senderPassword = _configuration["EmailSettings:SenderPassword"];
		var useStartTls = bool.TryParse(_configuration["EmailSettings:UseStartTls"], out bool result) ? result : true;

		using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
		{
			smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
			smtpClient.EnableSsl = false; // Important for Gmail
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtpClient.UseDefaultCredentials = false;
			smtpClient.TargetName = "STARTTLS/smtp.gmail.com"; // Ensures TLS encryption
			smtpClient.EnableSsl = useStartTls; // Use TLS encryption

			var mailMessage = new MailMessage
			{
				From = new MailAddress(senderEmail),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};

			mailMessage.To.Add(toEmail);

			try
			{
				await smtpClient.SendMailAsync(mailMessage);
			}
			catch (Exception ex)
			{
				throw new Exception($"Failed to send email: {ex.Message}");
			}
		}
	}
}