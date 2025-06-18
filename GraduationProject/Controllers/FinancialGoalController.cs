using Azure;
using gp.Models;
using Graduation_Project.DTO;
using Graduation_Project.Helping_Functions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Graduation_Project.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FinancialGoalController : BaseController
	{
		private readonly IConfiguration configuration;
		AppDbContext db;
		UserManager<User> userManager;
		private readonly HttpClient httpClient;
		private readonly IEmailService emailService;
		public FinancialGoalController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration,HttpClient httpClient,IEmailService emailService):base(db, emailService)
		{
			this.db = db;
			this.userManager = userManager;
			this.configuration = configuration;
			this.httpClient = httpClient;
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
		[Authorize]
		[HttpGet("UserFinancialGoal")]
		public async Task<IActionResult> GetuserFinancialgoal()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				NotFound("User Not Found");
			}
			SendUserFinancialGoalDTO dto = new SendUserFinancialGoalDTO
			{
				financialGoal = user.financialGoal,
				salary = user.salary
			};
			return Ok(dto);
		}
		[Authorize]
		[HttpGet("Testt")]
		public async Task<IActionResult> test()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var data= await TrackSpendingGoal(user);
			return Ok(
				);

		}

	}
}
