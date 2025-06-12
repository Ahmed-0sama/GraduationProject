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
		[HttpGet("GetBills")]
		public async Task<ActionResult<IEnumerable<MonthlyBill>>> GetBills()
		{
			return await db.MonthlyBills.ToListAsync();
		}
		[Authorize]
		[HttpGet("GetMonthlyBillsForUser")]
		public async Task<IActionResult> GetMonthlyBillsForUser()
		{
			var user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
			if (user == null)
			{
				return NotFound("User Not Found");
			}
			var bills = await db.MonthlyBills.Where(p => p.UserId == user.Id).ToListAsync();
			List<returnMonthlybills> dtoList = new List<returnMonthlybills>();
			foreach(var item in bills)
			{
				var dto = new returnMonthlybills
				{
					Issuer = item.Issuer,
					Name = item.Name,
					StartDate = item.StartDate,
					EndDate = item.EndDate,
					Category = item.Category
				};
				dtoList.Add(dto);
			}
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
		[HttpPost]
		public async Task<ActionResult<MonthlyBill>> CreateBill(AddMonthlyBill dto)
		{
			if (ModelState.IsValid)
			{
				var user= await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
				if (user == null)
				{
					return NotFound("User Not Found");
				}
				else
				{
					MonthlyBill bill = new MonthlyBill();
					var expenses = await db.Expenses.FirstOrDefaultAsync(p => p.userId == user.Id);
					bill.UserId = user.Id;
					bill.ExpenseId = expenses.ExpenseId;
					bill.Issuer = dto.Issuer;
					bill.Amount = dto.Amount;
					bill.StartDate = dto.StartDate;
					bill.EndDate = dto.EndDate;
					bill.Category = dto.Category;
					bill.Duration = dto.Duration;
					db.MonthlyBills.Add(bill);
					db.SaveChanges();
					await TrackSpendingGoal(user);
					return Ok("item saved");
				}
			}
			else
			{
				return NotFound(ModelState);
			}
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
