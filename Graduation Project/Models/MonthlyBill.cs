using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gp.Models;

public partial class MonthlyBill
{
    [Key]
	public int BillId { get; set; }

    public int? UserId { get; set; }

    public string? Name { get; set; }

    public string? Issuer { get; set; }

    public string? Category { get; set; }

    public double? Amount { get; set; }

    public int? Duration { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
	public int ExpenseId { get; set; }
	public virtual Expense Expense { get; set; }

	public virtual User? User { get; set; }
}
