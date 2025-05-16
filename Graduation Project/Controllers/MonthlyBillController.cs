using Azure;
using gp.Models;
using Graduation_Project.DTO;
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
	public class MonthlyBillController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		AppDbContext db;
		UserManager<User> userManager;
		List<string> AllowedCategories = new List<string> { "Clothes", "Electronics", " Food & Groceries", " Other" };
		public MonthlyBillController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration)
		{
			this.db = db;
			this.userManager = userManager;
			_configuration = configuration;
		}
		[Authorize]
		[HttpGet("User/{UserId}/expenses/Total")]
		public async Task<ActionResult<decimal>> GetTotalExpenses()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			var date = new DateTime(DateTime.UtcNow.Month);

			var TotalBills = await db.MonthlyBills
		.Where(p => p.UserId == user.Id && date >= p.StartDate && date <= p.EndDate)
		.SumAsync(p => p.Amount);
			return Ok(TotalBills);
		}
	}
}
