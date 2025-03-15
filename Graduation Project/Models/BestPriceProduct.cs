using Graduation_Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class BestPriceProduct
{
    [Key]
	public int ItemId { get; set; }
	

    public string? Category { get; set; }

    public string? Image { get; set; }

    public DateOnly? CurrentDate { get; set; }

    public double CurrentPrice { get; set; }

    public string? Url { get; set; }

    public int Quantity { get; set; }

    public string? ShopName { get; set; }

    public string? ProductName { get; set; }

    public bool? IsBought { get; set; }
    [ForeignKey("ListId")]
	public int ToBuyListID { get; set; }
    public virtual ToBuyList ToBuyList { get; set; }
    public virtual List<ProductPriceHistory> PriceHistory { get; set; } = new();
	public PurchasedProduct PurchasedProduct { get; set; }
}
