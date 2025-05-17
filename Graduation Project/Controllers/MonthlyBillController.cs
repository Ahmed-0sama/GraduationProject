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
	public class MonthlyBillController : BaseController
	{
		private readonly IConfiguration _configuration;
		AppDbContext db;
		UserManager<User> userManager;
		public MonthlyBillController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration,IEmailService emailService):base(db, emailService)
		{
			this.db = db;
			this.userManager = userManager;
			_configuration = configuration;
		}
		[Authorize]
		[HttpGet("MonthlyBills/Total")]
		public async Task<ActionResult<decimal>> GetTotalMonthlyBills()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			double TotalBills =await CalculateTotalMonthlyBillsByuser(user);
			return Ok(TotalBills);
		}
	}
}
