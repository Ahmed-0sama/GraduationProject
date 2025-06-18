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
		[Authorize]
		[HttpGet("GetMonthlyBillsForUser")]
		public async Task<IActionResult> GetMonthlyBillsForUser()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Unauthorized("User ID not found in token.");

			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound("User not found");

			var bills = await db.MonthlyBills
				.Where(p => p.UserId == user.Id)
				.ToListAsync();

			var dtoList = bills.Select(item => new ReturnMonthlyBills
			{
				Issuer = item.Issuer,
				Name = item.Name,
				StartDate = item.StartDate,
				EndDate = item.EndDate,
				Category = item.Category,
				Amount = item.Amount,
				Duration = item.Duration
			}).ToList();

			return Ok(dtoList);
		}
		[Authorize]
		[HttpGet("{id}")]
		public async Task<ActionResult<MonthlyBill>> GetBillWithId(int id)
		{
			var bill = await db.MonthlyBills.FindAsync(id);
			if (bill == null)
			{
				return NotFound("bill not found");
			}
			else
			{
				return Ok(bill);
			}
		}
		[Authorize]
		[HttpGet("GetBills")]
		public async Task<ActionResult<IEnumerable<MonthlyBill>>> GetBills()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Unauthorized("User ID not found in token.");

			var user = await userManager.FindByIdAsync(userId);
			if (user == null)
				return NotFound("User not found");

			var list = await db.MonthlyBills
		.Where(u => u.UserId == user.Id)
			.Select(b => new
			{
			b.Name,
			b.Issuer,
			b.Amount,
			b.StartDate,
			b.EndDate
			})
		.ToListAsync();

			return Ok(list);
		}
		[Authorize]
		[HttpPost]
		public async Task<ActionResult<MonthlyBill>> CreateBill(AddMonthlyBill dto)
		{
			if (ModelState.IsValid)
			{
				var user = await userManager.Users
					.Include(u => u.Expense)
					.FirstOrDefaultAsync(u => u.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

				if (user == null)
				{
					return NotFound("User Not Found");
				}

				var expenses = await db.Expenses.FirstOrDefaultAsync(e => e.User.Id == user.Id);
				if (expenses == null)
				{
					return BadRequest("No expenses found for user.");
				}

				MonthlyBill bill = new MonthlyBill
				{
					UserId = user.Id,
					ExpenseId = expenses.ExpenseId,
					Issuer = dto.Issuer,
					Name=dto.Name,
					Amount = dto.Amount,
					StartDate = dto.StartDate,
					EndDate = dto.EndDate,
					Category = dto.Category,
					Duration = dto.Duration
				};

				db.MonthlyBills.Add(bill);
				await db.SaveChangesAsync();
				await TrackSpendingGoal(user);

				return Ok("Item saved");
			}
			return BadRequest(ModelState);
		}
		[Authorize]
		[HttpGet("GetTotalBillsForLastMonth")]
		public async Task<IActionResult> GetBillslastmonth()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var currentDate = DateTime.UtcNow;

			var bills = await db.MonthlyBills
			.Where(u => u.UserId == user.Id && u.StartDate < currentDate && u.EndDate > currentDate)
			.ToListAsync();

			var total = bills.Sum(b => b.Amount);

			return Ok(new { Total = total });

		}
		[Authorize]
		[HttpGet("GetLastMonthBillsWithCategories")]
		public async Task<IActionResult> GetLastMonthBillsWithCategories()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}

			var today = DateTime.UtcNow;
			var lastMonth = today.AddMonths(-1);
			var monthStart = new DateTime(lastMonth.Year, lastMonth.Month, 1);
			var monthEnd = monthStart.AddMonths(1);

			var bills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id && b.StartDate < monthEnd && b.EndDate > monthStart)
				.ToListAsync();

			var total = bills.Sum(b => b.Amount);

			var billsByCategory = bills
				.GroupBy(b => b.Category)
				.ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));

			return Ok(new
			{
				Month = monthStart.ToString("yyyy-MM"),
				Total = total,
				Categories = billsByCategory
			});
		}
		[Authorize]
		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateBills(int id, MonthlyBill bill)
		{
			if (id != bill.BillId)
			{
				return BadRequest();
			}
			var Bill = db.MonthlyBills.Find(id);
			if (Bill == null)
			{
				return NotFound("Bill not found");
			}
			else
			{
				Bill.Duration = bill.Duration;
				Bill.Name = bill.Name;
				Bill.Issuer = bill.Issuer;
				Bill.BillId = bill.BillId;
				Bill.UserId = bill.UserId;
				Bill.Amount = bill.Amount;
				Bill.EndDate = bill.EndDate;
				Bill.StartDate = bill.StartDate;
				Bill.Category = bill.Category;
				Bill.ExpenseId = bill.ExpenseId;
				db.SaveChangesAsync();
				return Ok("update is done");
			}
		}
		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteBills(int id)
		{
			var bill = await db.MonthlyBills
				.Include(p => p.Expense)
				.FirstOrDefaultAsync(p => p.BillId == id);
			if (bill == null)
			{
				return NotFound("bill is not found");
			}
			else
			{
				int expensesId = bill.ExpenseId;
				db.MonthlyBills.Remove(bill);
				await db.SaveChangesAsync();
				bool hasOtherRelated = await db.MonthlyBills.AnyAsync(b => b.ExpenseId == expensesId) ||
					await db.PurchasedProducts.AnyAsync(p => p.ExpenseId == expensesId);
				if (!hasOtherRelated)
				{
					var expenses = await db.Expenses.FindAsync(expensesId);
					if (expenses != null)
					{
						db.Expenses.Remove(expenses);
						await db.SaveChangesAsync();
					}
				}
			}
			return NoContent();
		}
	}
}
