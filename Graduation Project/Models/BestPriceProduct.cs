using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class BestPriceProduct
{
    [Key]
	public int ItemId { get; set; }

    [ForeignKey(nameof(ListId))]
	public int ListId { get; set; }

    public string? Category { get; set; }

    public string? Image { get; set; }

    public DateOnly? Date { get; set; }

    public double Price { get; set; }

    public string? Url { get; set; }

    public int Quantity { get; set; }

    public string? ShopName { get; set; }

    public string? ProductName { get; set; }

    public bool? IsBought { get; set; }

    public virtual ToBuyList List { get; set; }
}
