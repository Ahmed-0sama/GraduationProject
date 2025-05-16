using gp.Models;
using Graduation_Project.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AlertController : ControllerBase
	{
		private readonly IConfiguration configuration;
		AppDbContext db;
		UserManager<User> userManager;
		public AlertController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration)
		{
			this.db = db;
			this.userManager = userManager;
			this.configuration = configuration;
		}
		[Authorize]
		[HttpPost("Send Alert")]
		public async Task<IActionResult> SendAlert(int type)
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			Alert alert = new Alert
			{
				UserId = user.Id,
				DateTime = DateTime.UtcNow
			};
			if (type == 50)
			{
				alert.Message = "Heads up! You’ve used 50% of your monthly budget. Smart spending starts now.";
				alert.Type = 50;
			}
			else if (type == 70)
			{
				alert.Message = "Warning: 70% of your spending limit is used. Consider reviewing your expenses.";
				alert.Type = 70;
			}
			else
			{
				alert.Message = "You've hit 100% of your spending limit. It's time to pause and reassess your spending.";
				alert.Type = 100;
			}
			db.Alerts.Add(alert);
			await db.SaveChangesAsync();
			return Ok("Alert sent successfully.");
		}
		[Authorize]
		[HttpGet]
		public async Task<IActionResult> GetAllAlertsForUser()
		{
			var user =await userManager.FindByIdAsync(User.FindFirstValue( ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}

			var allAlerts = db.Alerts
				   .Where(u => u.UserId == user.Id)
				   .OrderByDescending(a => a.DateTime) 
				   .ToList();
			var alertDTOs = new List<AllAlertsUserDTO>();
			foreach (var al in allAlerts)
			{
				alertDTOs.Add(new AllAlertsUserDTO
				{
					type = al.Type,
					Message = al.Message,
					Date = (DateTime)al.DateTime
				});
			}
			return Ok(alertDTOs);
		}

	}
}
