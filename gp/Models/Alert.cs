using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class Alert
{
    public int AlertId { get; set; }

    public int? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? DateTime { get; set; }

    public string? Type { get; set; }

    public virtual User? User { get; set; }
}
