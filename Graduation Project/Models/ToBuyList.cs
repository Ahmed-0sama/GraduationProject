using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gp.Models;

public partial class ToBuyList
{
    [Key]
	public int ListId { get; set; }

    public int? UserId { get; set; }

    public string? ProductName { get; set; }

    public DateTime? Date { get; set; }

    public virtual ICollection<BestPriceProduct> BestPriceProducts { get; set; } = new List<BestPriceProduct>();

    public virtual User? User { get; set; }
}
