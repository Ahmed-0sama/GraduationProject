using gp.Models;
using Graduation_Project.DTO;

using Graduation_Project.Models;
using Graduation_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FinancialGoalController : ControllerBase
	{
		private readonly IConfiguration configuration;
		AppDbContext db;
		UserManager<User> userManager;
		public FinancialGoalController(UserManager<User> userManager, AppDbContext db,IConfiguration configuration)
		{
			this.userManager = userManager;
			this.db = db;
			this.configuration = configuration;
		}
		[Authorize]
		[HttpPost("SetFinanicalGoal")]
		public async Task<IActionResult> SetFinanicalGoal(FinanicalGoalDTO financialgoaldto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
				if (user == null)
				{
					return NotFound("User Not Found");
				}
				user.salary = financialgoaldto.salary;
				user.financialGoal = financialgoaldto.SavingGoal;
				await db.SaveChangesAsync();
				return Ok("Financial Goal Set Sucessfully");
			}
			return BadRequest(ModelState);
		}
		[Authorize]
		[HttpPut("UpdateFinancialGoal")]
		public async Task<IActionResult> UpdateFinanicalGoal(FinanicalGoalDTO financialGoalDto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
				if (user == null)
				{
					return NotFound("User Not Found");
				}
				user.salary = financialGoalDto.salary;
				user.financialGoal = financialGoalDto.SavingGoal;
				await db.SaveChangesAsync();
				return Ok("Data Has been Updated");
			}
			return BadRequest(ModelState);
		}
	}
}
