using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gp.Models;

public partial class PurchasedProduct
{
    [Key]
	public int PurchasedId { get; set; }

    public string UserId { get; set; }

    public string Category { get; set; }

    public DateOnly Date { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public string ShopName { get; set; }

    public string ItemName { get; set; }

    public string? ReceiptImage { get; set; }

    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    public virtual User? User { get; set; }
}
