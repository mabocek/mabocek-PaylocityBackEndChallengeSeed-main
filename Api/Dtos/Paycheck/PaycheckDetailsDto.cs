namespace Api.Dtos.Paycheck;

/// <summary>
/// DTO providing detailed breakdown of paycheck benefit deductions.
/// Shows how the monthly benefit costs are calculated and distributed across paychecks.
/// </summary>
public record PaycheckDetailsDto(
    /// <summary>
    /// Employee's base monthly benefit cost: $1,000 per month
    /// Based on requirement: "employees have a base cost of $1,000 per month (for benefits)"
    /// </summary>
    decimal EmployeeBaseCost,

    /// <summary>
    /// Total monthly cost for all dependents
    /// Based on requirement: "each dependent represents an additional $600 cost per month (for benefits)"
    /// </summary>
    decimal DependentsCost,

    /// <summary>
    /// Additional monthly cost for high-salary employees
    /// Based on requirement: "employees that make more than $80,000 per year will incur an additional 2% of their yearly salary in benefits costs"
    /// Calculated as: (annual salary * 2%) / 12 months
    /// </summary>
    decimal HighSalaryAdditionalCost,

    /// <summary>
    /// Additional monthly cost for dependents over 50 years old
    /// Based on requirement: "dependents that are over 50 years old will incur an additional $200 per month"
    /// </summary>
    decimal SeniorDependentsCost,

    /// <summary>
    /// Total monthly benefit cost (sum of all monthly costs above)
    /// </summary>
    decimal TotalMonthlyCost,

    /// <summary>
    /// Benefit deduction per paycheck
    /// Based on requirement: "26 paychecks per year with deductions spread as evenly as possible on each paycheck"
    /// Calculated as: (TotalMonthlyCost * 12 months) / 26 paychecks
    /// </summary>
    decimal PerPaycheckDeduction
)
{
    /// <summary>
    /// Number of paychecks per year (always 26 per requirements)
    /// Based on requirement: "26 paychecks per year"
    /// </summary>
    public int PaychecksPerYear { get; init; } = 26;

    /// <summary>
    /// Detailed cost breakdown for each dependent
    /// Shows individual costs and age-based adjustments
    /// </summary>
    public ICollection<DependentCostBreakdownDto> DependentBreakdowns { get; init; } = new List<DependentCostBreakdownDto>();
};
