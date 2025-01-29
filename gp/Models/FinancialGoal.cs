using System;
using System.Collections.Generic;

namespace gp.Models;

public partial class FinancialGoal
{
    public int GoalId { get; set; }

    public int? UserId { get; set; }

    public double? MonthlySpendingLimit { get; set; }

    public double? SavingGoal { get; set; }

    public bool? Triger { get; set; }

    public virtual User? User { get; set; }
}
