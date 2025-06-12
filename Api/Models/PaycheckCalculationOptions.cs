namespace Api.Models;

/// <summary>
/// Configuration options for paycheck and benefit calculations.
/// Contains all business rule constants that can be configured.
/// </summary>
public class PaycheckCalculationOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "PaycheckCalculation";

    /// <summary>
    /// Base benefit cost per month for an employee
    /// </summary>
    public decimal BaseBenefitCostPerMonth { get; set; } = 1000m;

    /// <summary>
    /// Benefit cost per month for each dependent
    /// </summary>
    public decimal DependentBenefitCostPerMonth { get; set; } = 600m;

    /// <summary>
    /// Salary threshold above which additional benefit costs apply
    /// </summary>
    public decimal HighSalaryThreshold { get; set; } = 80000m;

    /// <summary>
    /// Additional benefit cost percentage for high salary employees
    /// </summary>
    public decimal HighSalaryAdditionalCostPercentage { get; set; } = 0.02m;

    /// <summary>
    /// Age threshold for dependents to be considered seniors
    /// </summary>
    public decimal SeniorDependentThreshold { get; set; } = 50;

    /// <summary>
    /// Additional monthly cost for senior dependents
    /// </summary>
    public decimal SeniorDependentAdditionalCostPerMonth { get; set; } = 200m;

    /// <summary>
    /// Number of paychecks per year
    /// </summary>
    public int PaychecksPerYear { get; set; } = 26;
}
