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
	public class ExpensesController : ControllerBase
	{
		private readonly IConfiguration configuration;
		AppDbContext db;
		UserManager<User> userManager;
		public ExpensesController(AppDbContext db, UserManager<User> userManager, IConfiguration configuration)
		{
			this.db = db;
			this.userManager = userManager;
			this.configuration = configuration;
		}
		[HttpGet("GetTotalExpenses")]
		public async Task<IActionResult> GetTotalExpenses()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("user not found");
			}

			var currentMonth = DateTime.UtcNow.Month;
			var currentYear = DateTime.UtcNow.Year;
			var monthStart = new DateTime(currentYear, currentMonth, 1);
			var monthEnd = monthStart.AddMonths(1).AddDays(-1);

			decimal totalSpending = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id &&
							p.Date.Month == currentMonth &&
							p.Date.Year == currentYear)
				.SumAsync(p => (decimal)(p.Price * p.Quantity));

			decimal totalBills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id &&
							b.EndDate >= monthStart &&
							b.StartDate <= monthEnd)
				.SumAsync(b => (decimal)b.Amount);
			return Ok(new
			{
				TotalSpending = totalSpending,
				TotalBills = totalBills,
				TotalExpenses = totalSpending + totalBills
			});
		}
		[Authorize]
		[HttpGet("GetAllExpenses")]
		public async Task<ActionResult<IEnumerable<GetExpensesDTO>>> GetAllExpenses()
		{
			var expenses = await db.Expenses
				.Include(e => e.PurchasedItems)
				.ToListAsync();

			List<GetExpensesDTO> dto = new List<GetExpensesDTO>();

			foreach (var ex in expenses)
			{
				dto.Add(new GetExpensesDTO
				{
					userId = ex.userId,
					expensesid = ex.ExpenseId
				});
			}

			return Ok(dto);
		}
		[Authorize]
		[HttpGet("id")]
		public async Task<ActionResult<GetExpensesDTO>> GetExpenses(int id)
		{
			var expenses = await db.Expenses
				.FirstOrDefaultAsync(e => e.ExpenseId == id);
			if (expenses == null)
			{
				return NotFound("not foun expenses to show it");
			}
			GetExpensesDTO dto = new GetExpensesDTO();
			dto.userId = expenses.userId;
			dto.expensesid = expenses.ExpenseId;
			return dto;
		}
		[Authorize]
		[HttpPost]
		public async Task<ActionResult<Expense>> CreateExpenses(Expense expense)
		{
			db.Expenses.Add(expense);
			await db.SaveChangesAsync();
			return CreatedAtAction(nameof(GetExpenses), new { id = expense.ExpenseId }, expense);
		}
		[Authorize]
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteExpenses(int id)
		{
			var expense = await db.Expenses.FindAsync(id);
			if (expense == null)
			{
				return NotFound("not found expenses for this user");
			}
			var relatedPurchsedProduct = db.PurchasedProducts.Where(e => e.ExpenseId == expense.ExpenseId);
			db.PurchasedProducts.RemoveRange(relatedPurchsedProduct);
			db.Expenses.Remove(expense);
			await db.SaveChangesAsync();

			return NoContent();
		}
		[Authorize]
		[HttpGet("UserHaseExpenses")]
		public async Task<ActionResult<bool>> UserHasExpenses()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User not found");
			}
			else
			{
				bool HasExpenses = await db.Expenses.AnyAsync(p => p.userId == user.Id);
				return Ok(HasExpenses);
			}
		}
		[Authorize]
		[HttpGet("lastSevenMonthes")]
		public async Task<IActionResult> lastsevenmonthes()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var currentDate = DateTime.UtcNow.Date;
			var monthCutoff = new DateTime(currentDate.AddMonths(-6).Year, currentDate.AddMonths(-6).Month, 1);
			var cutoffDateOnly = DateOnly.FromDateTime(monthCutoff);
			var allBills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id && b.EndDate > monthCutoff)
				.ToListAsync();

			var allProducts = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id && p.Date >= cutoffDateOnly)
				.ToListAsync();

			var result = new List<object>();

			for (int i = 0; i < 7; i++)
			{
				var monthDate = currentDate.AddMonths(-i);
				var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
				var monthEnd = monthStart.AddMonths(1);

				var startDateOnly = DateOnly.FromDateTime(monthStart);
				var endDateOnly = DateOnly.FromDateTime(monthEnd);

				var monthBills = allBills
					.Where(b => b.StartDate < monthEnd && b.EndDate > monthStart)
					.Sum(b => b.Amount);

				var monthProducts = allProducts
					.Where(p => p.Date >= startDateOnly && p.Date < endDateOnly)
					.Sum(p => p.Price);

				result.Add(new
				{
					Month = monthStart.ToString("yyyy-MM"),
					Total = monthBills + monthProducts
				});
			}
			var sorted = result
				.Cast<dynamic>()
				.OrderBy(r => r.Month)
				.ToList();

			return Ok(sorted);
		}
		[Authorize]
		[HttpGet("lastSevenMonthsWithCategories")]
		public async Task<IActionResult> lastSevenMonthsWithCategories()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var currentDate = DateTime.UtcNow.Date;
			var monthCutoff = new DateTime(currentDate.AddMonths(-6).Year, currentDate.AddMonths(-6).Month, 1);
			var result = new List<dynamic>();
			var cutoffDateOnly = DateOnly.FromDateTime(monthCutoff);

			var allBills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id && b.EndDate > monthCutoff)
				.ToListAsync();

			var allProducts = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id && p.Date >= cutoffDateOnly)
				.ToListAsync();

			for (int i = 0; i < 7; i++)
			{
				var monthDate = currentDate.AddMonths(-i);
				var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
				var monthEnd = monthStart.AddMonths(1);
				var monthStartDateOnly = DateOnly.FromDateTime(monthStart);
				var monthEndDateOnly = DateOnly.FromDateTime(monthEnd);

				var bills = allBills
					.Where(b => b.StartDate < monthEnd && b.EndDate > monthStart)
					.ToList();
				var billSum = bills.Sum(b => b.Amount);

				var products = allProducts
					.Where(p => p.Date >= monthStartDateOnly && p.Date < monthEndDateOnly)
					.ToList();
				var productSum = products.Sum(p => p.Price);

				var billCategories = bills
					.GroupBy(b => b.Category)
					.ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));

				var productCategories = products
					.GroupBy(p => p.Category)
					.ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

				result.Add(new
				{
					Month = monthStart.ToString("yyyy-MM"),
					BillSum = billSum,
					ProductSum = productSum,
					Total = billSum + productSum,
					BillsCategory = billCategories,
					ProductsCategory = productCategories
				});
			}
			return Ok(result.OrderBy(r => r.Month));
		
		}
		[Authorize]
		[HttpGet("lastSevenDaysWithCategories")]
		public async Task<IActionResult> LastSevenDaysWithCategories()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}

			var currentDate = DateTime.UtcNow.Date;
			var dayCutoff = currentDate.AddDays(-6);
			var result = new List<dynamic>();

			var allBills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id && b.EndDate >= dayCutoff)
				.ToListAsync();

			var allProducts = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id && p.Date >= DateOnly.FromDateTime(dayCutoff))
				.ToListAsync();

			for (int i = 0; i < 7; i++)
			{
				var day = currentDate.AddDays(-i);
				var dayStart = day;
				var dayEnd = day.AddDays(1);
				var dayStartDateOnly = DateOnly.FromDateTime(dayStart);
				var dayEndDateOnly = DateOnly.FromDateTime(dayEnd);

				var bills = allBills
					.Where(b => b.StartDate <= dayEnd && b.EndDate >= dayStart)
					.ToList();
				var billSum = bills.Sum(b => b.Amount);

				var products = allProducts
					.Where(p => p.Date >= dayStartDateOnly && p.Date < dayEndDateOnly)
					.ToList();
				var productSum = products.Sum(p => p.Price);

				var billCategories = bills
					.GroupBy(b => b.Category)
					.ToDictionary(g => g.Key, g => g.Sum(b => b.Amount));

				var productCategories = products
					.GroupBy(p => p.Category)
					.ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

				result.Add(new
				{
					Day = day.ToString("yyyy-MM-dd"),
					BillSum = billSum,
					ProductSum = productSum,
					Total = billSum + productSum,
					BillsCategory = billCategories,
					ProductsCategory = productCategories
				});
			}

			return Ok(result.OrderBy(r => r.Day));
		}
	}

}
