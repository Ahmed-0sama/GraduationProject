using Google;
using gp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Graduation_Project.Helping_Functions
{
	public class BaseController : Controller
	{
		protected readonly AppDbContext db;
		private readonly IEmailService emailService;
		public BaseController(AppDbContext context, IEmailService emailService)
		{
			db = context;
			this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
		}

		protected async Task<double> CalculateTotalPurchasedByUser(User user)
		{
			var currentMonth = DateTime.UtcNow.Month;
			var currentYear = DateTime.UtcNow.Year;

			double total = await db.PurchasedProducts
				.Where(p => p.UserId == user.Id && p.Date.Month == currentMonth && p.Date.Year == currentYear)
				.SumAsync(p => p.Price * p.Quantity);

			return total;
		}
		protected async Task<double> CalculateTotalMonthlyBillsByuser(User user)
		{
			var currentDate = DateTime.UtcNow;
			var currentMonthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
			var nextMonthStart = currentMonthStart.AddMonths(1);

			double totalBills = (double)await db.MonthlyBills
				.Where(p => p.UserId == user.Id &&
							p.StartDate.HasValue &&
							p.EndDate.HasValue &&
							p.StartDate < nextMonthStart &&
							p.EndDate >= currentMonthStart)
				.SumAsync(p => p.Amount);

			return totalBills;
		}
		protected async Task<Alert> TrackSpendingGoal(User user)
		{
			if (user.financialGoal == null || user.financialGoal == 0)
			{
				return null;
			}
			var totalSpending = await CalculateTotalPurchasedByUser(user);
			var monthlyBills = await CalculateTotalMonthlyBillsByuser(user);
			var total = totalSpending + monthlyBills;

			decimal financialGoal = (decimal)user.financialGoal;
			decimal percent = ((decimal)total / financialGoal) * 100;
			Alert alert = null;
			if (percent >= 100)
			{
				alert = new Alert
				{
					Message = "You've hit 100% of your spending limit. It's time to pause and reassess your spending.",
					UserId = user.Id,
					DateTime = DateTime.UtcNow,
					Type = 100
				};
			}
			else if (percent >= 70)
			{
				alert = new Alert
				{
					Message = "Warning: 70% of your spending limit is used. Consider reviewing your expenses.",
					UserId = user.Id,
					DateTime = DateTime.UtcNow,
					Type = 70
				};

			}
			else if (percent >= 50)
			{
				alert = new Alert
				{
					Message = "Heads up! You’ve used 50% of your monthly budget. Smart spending starts now.",
					UserId = user.Id,
					DateTime = DateTime.UtcNow,
					Type = 50
				};

			}
			if (alert != null)
			{
				db.Alerts.Add(alert);
				await db.SaveChangesAsync();
			}
			await emailService.SendEmailAsync(user.Email, "Spending Alert", alert.Message);
			return alert;
		}
	}
}
