using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gp.Models;

public partial class FinancialGoal
{
    [Key]
	public int GoalId { get; set; }

    public int? UserId { get; set; }

    public double? MonthlySpendingLimit { get; set; }

    public double? SavingGoal { get; set; }

    public virtual User? User { get; set; }
}
