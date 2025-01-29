using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public int? UserId { get; set; }

    public DateOnly? Date { get; set; }

    public double? Amount { get; set; }

    public string? Category { get; set; }

    public string? Type { get; set; }

    public string? Details { get; set; }

    public virtual ICollection<MonthlyBill> MonthlyBills { get; set; } = new List<MonthlyBill>();

    public virtual ICollection<PurchasedProduct> PurchasedProducts { get; set; } = new List<PurchasedProduct>();

    public virtual User? User { get; set; }
}
