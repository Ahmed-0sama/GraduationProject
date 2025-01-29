using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class ToBuyList
{
    public int ListId { get; set; }

    public int? UserId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductImage { get; set; }

    public virtual User? User { get; set; }
}
