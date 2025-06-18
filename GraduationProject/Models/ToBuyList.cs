using Graduation_Project.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class ToBuyList
{
    [Key]
	public int ListId { get; set; }
    public string ProductName { get; set; }
	public virtual ICollection<BestPriceProduct> BestPriceProducts { get; set; } = new List<BestPriceProduct>();
	public virtual ICollection<UserToBuyList> UserToBuyLists { get; set; } = new List<UserToBuyList>();

}
