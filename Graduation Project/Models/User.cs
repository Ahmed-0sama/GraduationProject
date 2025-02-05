using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class User:IdentityUser
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }
    public string? lastName { get; set; }
	
	//public string Email { get; set; }
    public string? Photo { get; set; }

    public bool? IsAdmin { get; set; }

    public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual ICollection<FinancialGoal> FinancialGoals { get; set; } = new List<FinancialGoal>();

    public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();

    public virtual ICollection<PurchasedProduct> PurchasedProducts { get; set; } = new List<PurchasedProduct>();

    public virtual ICollection<ToBuyList> ToBuyLists { get; set; } = new List<ToBuyList>();
}
