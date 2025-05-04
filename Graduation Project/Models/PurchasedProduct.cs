using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public class PurchasedProduct
{
    [Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int PurchasedId { get; set; }

    public string UserId { get; set; }

    public string Category { get; set; }

    public DateOnly Date { get; set; }

    public double Price { get; set; }

    public int Quantity { get; set; }

    public string ShopName { get; set; }

    public string ItemName { get; set; }

    public string? ReceiptImage { get; set; }

	public int ExpenseId { get; set; }
	public virtual Expense Expense { get; set; }
	public int ? bestpriceproductId { get; set; }
	public virtual BestPriceProduct bestPriceProduct { get; set; }
	public virtual User? User { get; set; }
}
