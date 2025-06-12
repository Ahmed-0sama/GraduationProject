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
		[Authorize]
		[HttpGet("GetTotalExpenses")]
		public async Task<IActionResult> GetTotalExpenses()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("user not found");
			}
			var CurrentMonth = DateTime.UtcNow.Month;
			var CurrentYear = DateTime.UtcNow.Year;
			decimal TotalSpending = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id && p.Date.Month == CurrentMonth && p.Date.Year == CurrentYear)
				.SumAsync(p => (decimal)(p.Price * p.Quantity));
			var MonthStart = new DateTime(CurrentYear, CurrentMonth, 1);
			var MonthEnd = MonthStart.AddMonths(1).AddDays(-1);
			decimal TotalBills = await db.MonthlyBills
				.Where(b => b.UserId == user.Id && MonthStart >= b.StartDate && MonthStart <= b.EndDate)
				.SumAsync(b => (decimal)b.Amount);
			decimal TotalExpenses = TotalSpending + TotalBills;
			return Ok(TotalExpenses);
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
	}
}
