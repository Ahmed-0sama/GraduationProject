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
            decimal salary = (decimal)user.salary;
            decimal diff = (salary - financialGoal);
			decimal percent = ((decimal)total / diff) * 100;
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
					Message = "Heads up! You've used 50% of your monthly budget. Smart spending starts now.",
					UserId = user.Id,
					DateTime = DateTime.UtcNow,
					Type = 50
				};
			}
			if (alert != null)
            {
				db.Alerts.Add(alert);
				await db.SaveChangesAsync();

				// Generate HTML email content
				string htmlContent = GenerateSpendingAlertHtml(alert.Type, user,
	            (decimal)totalSpending, (decimal)monthlyBills, (decimal)total, financialGoal, percent,diff);

				// Send HTML email
				await emailService.SendEmailAsync(user.Email, "Spending Alert", htmlContent);
				return alert;
			}
            return null;
        }
	private string GenerateSpendingAlertHtml(int alertType, User user, decimal totalSpending, decimal monthlyBills, decimal total, decimal financialGoal, decimal percent,decimal diff)
        {
            string headerClass, iconClass, progressClass, headerTitle, icon, message, footerMessage;

            switch (alertType)
            {
                case 50:
                    headerClass = "alert-50";
                    iconClass = "icon-50";
                    progressClass = "progress-50";
                    headerTitle = "💰 Spending Alert";
                    icon = "📊";
                    message = $"Heads up! You've used <strong>{percent:F0}%</strong> of your monthly budget. Smart spending starts now.";
                    footerMessage = "Stay on track with your financial goals. You've got this! 💪";
                    break;
                case 70:
                    headerClass = "alert-70";
                    iconClass = "icon-70";
                    progressClass = "progress-70";
                    headerTitle = "⚠️ Spending Warning";
                    icon = "⚠️";
                    message = $"Warning: <strong>{percent:F0}%</strong> of your spending limit is used. Consider reviewing your expenses.";
                    footerMessage = "Time to slow down and make mindful spending choices. 🎯";
                    break;
                case 100:
                    headerClass = "alert-100";
                    iconClass = "icon-100";
                    progressClass = "progress-100";
                    headerTitle = "🚨 Budget Limit Reached";
                    icon = "🚨";
                    message = $"You've hit <strong>{percent:F0}%</strong> of your spending limit. It's time to pause and reassess your spending.";
                    footerMessage = "Don't worry - we're here to help you get back on track! 🤝";
                    break;
                default:
                    return string.Empty;
            }

            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Spending Alert</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }}
        
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background: white;
            border-radius: 12px;
            box-shadow: 0 4px 20px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        
        .header {{
            color: white;
            padding: 30px;
            text-align: center;
        }}
        
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: 600;
        }}
        
        .content {{
            padding: 40px 30px;
        }}
        
        .alert-50 {{ background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); }}
        .alert-70 {{ background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); }}
        .alert-100 {{ background: linear-gradient(135deg, #f44336 0%, #d32f2f 100%); }}
        
        .progress-container {{
            background: #f0f0f0;
            border-radius: 25px;
            height: 20px;
            margin: 20px 0;
            overflow: hidden;
        }}
        
        .progress-bar {{
            height: 100%;
            border-radius: 25px;
            transition: width 0.3s ease;
        }}
        
        .progress-50 {{ 
            background: linear-gradient(90deg, #4CAF50, #45a049);
            width: 50%;
        }}
        
        .progress-70 {{ 
            background: linear-gradient(90deg, #ff9800, #f57c00);
            width: 70%;
        }}
        
        .progress-100 {{ 
            background: linear-gradient(90deg, #f44336, #d32f2f);
            width: 100%;
        }}
        
        .percentage {{
            font-size: 36px;
            font-weight: bold;
            text-align: center;
            margin: 20px 0;
            color: #333;
        }}
        
        .message {{
            font-size: 18px;
            line-height: 1.6;
            color: #555;
            text-align: center;
            margin: 25px 0;
        }}
        
        .spending-details {{
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin: 25px 0;
        }}
        
        .spending-row {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            padding: 8px 0;
            border-bottom: 1px solid #e9ecef;
        }}
        
        .spending-row:last-child {{
            border-bottom: none;
            font-weight: bold;
            color: #333;
        }}
        
        .icon {{
            width: 60px;
            height: 60px;
            margin: 0 auto 20px;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 24px;
        }}
        
        .icon-50 {{ background: rgba(76, 175, 80, 0.2); color: #4CAF50; }}
        .icon-70 {{ background: rgba(255, 152, 0, 0.2); color: #ff9800; }}
        .icon-100 {{ background: rgba(244, 67, 54, 0.2); color: #f44336; }}
        
        .footer {{
            background: #f8f9fa;
            padding: 20px 30px;
            text-align: center;
            color: #666;
            font-size: 14px;
        }}
        
        .cta-button {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 15px 30px;
            border: none;
            border-radius: 25px;
            font-size: 16px;
            font-weight: 600;
            text-decoration: none;
            display: inline-block;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header {headerClass}'>
            <h1>{headerTitle}</h1>
        </div>
        <div class='content'>
            <div class='icon {iconClass}'>{icon}</div>
            <div class='percentage'>{percent:F0}%</div>
            <div class='progress-container'>
                <div class='progress-bar {progressClass}'></div>
            </div>
            <div class='message'>
                {message}
            </div>
            <div class='spending-details'>
                <div class='spending-row'>
                    <span>Monthly Budget:</span>
                    <span>L.E{diff:F2}</span>
                </div>
                <div class='spending-row'>
                    <span>Purchases:</span>
                    <span>L.E{totalSpending:F2}</span>
                </div>
                <div class='spending-row'>
                    <span>Monthly Bills:</span>
                    <span>L.E{monthlyBills:F2}</span>
                </div>
                <div class='spending-row'>
                    <span>Total Spent:</span>
                    <span>L.E{total:F2}</span>
                </div>
            </div>
        </div>
        <div class='footer'>
            <p>{footerMessage}</p>
            <p>Need help? Contact our support team anytime.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
