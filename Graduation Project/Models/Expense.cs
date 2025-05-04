using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class Expense
{
    [Key]
	public int ExpenseId { get; set; }
	public string UserId { get; set; }
	public virtual User User { get; set; }
	public virtual ICollection<PurchasedProduct> PurchasedItems { get; set; } = new List<PurchasedProduct>();

	public virtual ICollection<MonthlyBill> Bills { get; set; } = new List<MonthlyBill>();

}
