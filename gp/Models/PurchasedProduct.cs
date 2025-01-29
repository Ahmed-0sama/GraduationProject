using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class PurchasedProduct
{
    public int ItemId { get; set; }

    public int? ExpenseId { get; set; }

    public string? Category { get; set; }

    public DateOnly? Date { get; set; }

    public double? Price { get; set; }

    public int? Quantity { get; set; }

    public string? ShopName { get; set; }

    public string? ItemName { get; set; }

    public string? ReceiptImage { get; set; }

    public virtual Expense? Expense { get; set; }
}
