using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class Expense
{
    public int ExpenseId { get; set; }

    public int? PurchasedId { get; set; }

    public int? BillId { get; set; }

    public int? UserId { get; set; }

    public virtual MonthlyBill? Bill { get; set; }

    public virtual PurchasedProduct? Purchased { get; set; }

    public virtual User? User { get; set; }
}
