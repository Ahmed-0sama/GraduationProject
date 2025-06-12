using Graduation_Project.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
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
			if (user == null)
				return NotFound("User not found");

			var token = await userManager.GeneratePasswordResetTokenAsync(user);
			var resetLink = $"{configuration["AppUrl"]}/reset-password?email={model.Email}&token={Uri.EscapeDataString(token)}";

			// Send email logic (Replace with actual email service)
			await _emailService.SendEmailAsync(model.Email, "Reset Password", $"Click the link to reset your password: {resetLink}");

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

			var decodedToken = HttpUtility.UrlDecode(model.Token);

			var result = await userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

			if (!result.Succeeded)
				return BadRequest(result.Errors.Select(e => e.Description));

			return Ok("Password has been reset successfully.");
		}
	}
}
