using Graduation_Project.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;
using static EmailService;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MailController : ControllerBase
	{
		private readonly UserManager<gp.Models.User> userManager;
		private readonly IConfiguration configuration;
		private readonly IEmailService _emailService;
		public MailController(UserManager<gp.Models.User> userManager, IConfiguration configuration, IEmailService emailService)
		{
			this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			this._emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		}
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO model)
		{
			if (string.IsNullOrEmpty(model.Email))
				return BadRequest("Email is required");

			var user = await userManager.FindByEmailAsync(model.Email);
			if (user != null)
			{
				var token = await userManager.GeneratePasswordResetTokenAsync(user);

				// Use Base64 encoding for better token handling
				var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

				var resetLink = $"{configuration["FrontendUrl"]}/reset-password?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(encodedToken)}";

				// Send professional email
				await SendPasswordResetEmail(model.Email, resetLink, user.FirstName ?? "User");
			}
			return Ok("Password reset link has been sent to your email.");
		}
		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO model)
		{
			if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.NewPassword))
				return BadRequest("Invalid request");

			var user = await userManager.FindByEmailAsync(model.Email);
			if (user == null)
				return NotFound("User not found");

			string decodedToken;
			try
			{
				decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(model.Token)));
			}
			catch
			{
				return BadRequest(new { message = "Invalid token format." });
			}

			var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

			if (!result.Succeeded)
				return BadRequest(result.Errors.Select(e => e.Description));

			return Ok("Password has been reset successfully.");
		}
		private async Task SendPasswordResetEmail(string email, string resetLink, string userName)
		{
			var appName = configuration["Ecofy"] ?? "Ecofy";
			var companyName = configuration["Ecofy"] ?? "Ecofy";

			var subject = $"Reset Your {appName} Password";

			var htmlBody = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8'>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                .content {{ background-color: #f8f9fa; padding: 30px; border-radius: 0 0 8px 8px; }}
                .button {{ display: inline-block; background: linear-gradient(45deg, #007bff, #0056b3); color: white; padding: 15px 30px; text-decoration: none; border-radius: 25px; margin: 20px 0; font-weight: bold; box-shadow: 0 4px 15px rgba(0,123,255,0.3); }}
                .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
                .footer {{ text-align: center; margin-top: 30px; font-size: 12px; color: #666; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1 style='margin: 0; font-size: 28px;'>{appName}</h1>
                    <p style='margin: 10px 0 0 0; opacity: 0.9;'>Password Reset Request</p>
                </div>
                <div class='content'>
                    <h2 style='color: #333; margin-top: 0;'>Hello {userName},</h2>
                    
                    <p>We received a request to reset your password. Click the button below to create a new password:</p>
                    
                    <div style='text-align: center; margin: 30px 0;'>
                       <a href='{resetLink}' class='button' style='color: white;'>Reset My Password</a>
                    </div>
                    
                    <div class='warning'>
                        <strong>⚠️ Security Notice:</strong>
                        <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                            <li>This link expires in 24 hours</li>
                            <li>If you didn't request this, ignore this email</li>
                            <li>Never share this link with anyone</li>
                        </ul>
                    </div>
                    
                    <p><strong>Having trouble?</strong> Copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; background-color: #e9ecef; padding: 15px; border-radius: 5px; font-family: monospace; font-size: 12px;'>{resetLink}</p>
                    
                    <hr style='border: none; border-top: 1px solid #dee2e6; margin: 30px 0;'>
                    
                    <p style='margin-bottom: 0;'>Best regards,<br><strong>The {appName} Team</strong></p>
                </div>
                <div class='footer'>
                    <p>&copy; 2024 {companyName}. All rights reserved.</p>
                    <p style='margin: 5px 0 0 0;'>This is an automated message, please do not reply.</p>
                </div>
            </div>
        </body>
        </html>";

			await _emailService.SendEmailAsync(email, subject, htmlBody);
		}
	}
}
