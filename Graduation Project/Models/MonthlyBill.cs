using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class MonthlyBill
{
    [Key]
	public int BillId { get; set; }

    public string? UserId { get; set; }

    public string? Name { get; set; }

    public string? Issuer { get; set; }

    public string? Category { get; set; }

    public double? Amount { get; set; }

    public int? Duration { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    [ForeignKey("ExpenseId")]
	public int ExpenseId { get; set; }
	public virtual Expense Expense { get; set; }

	public virtual User? User { get; set; }
}
