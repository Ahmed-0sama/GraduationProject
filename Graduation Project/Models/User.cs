using Graduation_Project.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gp.Models;

public partial class User:IdentityUser
{
    [Key]
    public string? FirstName { get; set; }
    public string? lastName { get; set; }
    public byte[]? Photo { get; set; }
    public float?salary { get; set; }
    public float? financialGoal { get; set; }
    public bool? IsAdmin { get; set; }
    public string RefreshToken { get; set;}
    public DateTime RefreshTokenExpirytime { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

	public virtual Expense Expense { get; set; }
	//public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();

    public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();

    public virtual ICollection<PurchasedProduct> PurchasedProducts { get; set; } = new List<PurchasedProduct>();

	public virtual ICollection<UserToBuyList> UserToBuyLists { get; set; } = new List<UserToBuyList>();
}
