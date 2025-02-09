using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gp.Models;

public partial class Alert
{
    [Key]
	public int AlertId { get; set; }
    
    public string? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? DateTime { get; set; }

    public string? Type { get; set; }

    public virtual User? User { get; set; }
}
