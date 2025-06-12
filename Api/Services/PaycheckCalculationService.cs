using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Features;
using Api.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Api.Services;

/// <summary>
/// Service responsible for paycheck and benefit calculations.
/// Contains business logic for calculating benefits, deductions, and paycheck amounts.
/// </summary>
public class PaycheckCalculationService : IPaycheckCalculationService
{
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<PaycheckCalculationService> _logger;
    private readonly PaycheckCalculationOptions _options;

    // Constant that doesn't need configuration
    public const int MonthsPerYear = 12;

    public PaycheckCalculationService(
        IFeatureManager featureManager,
        ILogger<PaycheckCalculationService> logger,
        IOptions<PaycheckCalculationOptions> options)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Calculates paycheck for an employee based on business requirements.
    /// Implementation follows all benefit calculation rules from requirements.
    /// </summary>
    /// <param name="employee">The employee to calculate paycheck for</param>
    /// <returns>Calculated paycheck with detailed breakdown</returns>
    public async Task<GetPaycheckDto> CalculatePaycheckAsync(GetEmployeeDto employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        _logger.LogDebug("Calculating paycheck for employee {EmployeeId}", employee.Id);

        // Calculate gross pay per paycheck
        var grossPayPerPaycheck = CalculateGrossPayPerPaycheck(employee.Salary);

        // Calculate benefit costs following business rules
        var details = await CalculateBenefitDetailsAsync(employee);

        var paycheck = new GetPaycheckDto(
            employee.Id,
            $"{employee.FirstName} {employee.LastName}",
            grossPayPerPaycheck,
            details.PerPaycheckDeduction,
            grossPayPerPaycheck - details.PerPaycheckDeduction,
            details
        );

        _logger.LogDebug("Paycheck calculated for employee {EmployeeId}: Gross={GrossPay}, Deductions={Deductions}, Net={NetPay}",
            employee.Id, paycheck.GrossPay, paycheck.BenefitDeductions, paycheck.NetPay);

        return paycheck;
    }

    /// <summary>
    /// Calculates gross pay per paycheck based on annual salary.
    /// Based on requirement: "26 paychecks per year"
    /// </summary>
    /// <param name="annualSalary">Employee's annual salary</param>
    /// <returns>Gross pay per paycheck</returns>
    public decimal CalculateGrossPayPerPaycheck(decimal annualSalary)
    {
        if (annualSalary < 0)
            throw new ArgumentException("Annual salary cannot be negative", nameof(annualSalary));

        return annualSalary / _options.PaychecksPerYear;
    }

    /// <summary>
    /// Calculates benefit cost details for an employee
    /// </summary>
    /// <param name="employee">Employee to calculate benefits for</param>
    /// <returns>Detailed benefit cost breakdown</returns>
    public async Task<PaycheckDetailsDto> CalculateBenefitDetailsAsync(GetEmployeeDto employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        // Calculate base benefit costs
        var totalMonthlyCost = _options.BaseBenefitCostPerMonth;

        // Calculate dependent costs and create breakdown
        var dependentBreakdowns = new List<DependentCostBreakdownDto>();
        var totalDependentCost = 0m;
        var totalSeniorCost = 0m;

        foreach (var dependent in employee.Dependents)
        {
            var dependentCostInfo = await CalculateDependentCostAsync(dependent);
            totalDependentCost += dependentCostInfo.BaseCost;
            totalSeniorCost += dependentCostInfo.SeniorAdditionalCost;
            totalMonthlyCost += dependentCostInfo.TotalCost;

            // Only include detailed breakdown if feature is enabled
            if (await _featureManager.IsEnabledAsync(FeatureFlags.EnableDetailedPaycheckBreakdown))
            {
                dependentBreakdowns.Add(dependentCostInfo);
            }
        }

        // Calculate high salary additional cost if feature is enabled
        var highSalaryAdditionalCost = await CalculateHighSalaryAdditionalCostAsync(employee.Salary);
        totalMonthlyCost += highSalaryAdditionalCost;

        // Convert to per-paycheck amounts
        var perPaycheckDeduction = CalculatePerPaycheckDeduction(totalMonthlyCost);

        return new PaycheckDetailsDto(
            _options.BaseBenefitCostPerMonth,
            totalDependentCost,
            highSalaryAdditionalCost,
            totalSeniorCost,
            totalMonthlyCost,
            perPaycheckDeduction
        )
        {
            DependentBreakdowns = dependentBreakdowns
        };
    }

    /// <summary>
    /// Calculates cost breakdown for a single dependent
    /// </summary>
    /// <param name="dependent">Dependent to calculate cost for</param>
    /// <returns>Dependent cost breakdown</returns>
    public async Task<DependentCostBreakdownDto> CalculateDependentCostAsync(GetDependentDto dependent)
    {
        if (dependent == null)
            throw new ArgumentNullException(nameof(dependent));

        var dependentCost = _options.DependentBenefitCostPerMonth;
        var age = CalculateAge(dependent.DateOfBirth);
        var seniorAdditionalCost = 0m;

        // Apply senior dependent surcharge if feature is enabled
        if (age >= _options.SeniorDependentThreshold &&
            await _featureManager.IsEnabledAsync(FeatureFlags.EnableSeniorDependentSurcharge))
        {
            seniorAdditionalCost = _options.SeniorDependentAdditionalCostPerMonth;
            dependentCost += seniorAdditionalCost;
        }

        return new DependentCostBreakdownDto(
            dependent.Id,
            $"{dependent.FirstName} {dependent.LastName}",
            dependent.Relationship,
            (int)age,
            _options.DependentBenefitCostPerMonth,
            seniorAdditionalCost,
            dependentCost
        );
    }

    /// <summary>
    /// Calculates additional cost for high salary employees
    /// </summary>
    /// <param name="salary">Employee's annual salary</param>
    /// <returns>Additional monthly cost for high salary employees</returns>
    public async Task<decimal> CalculateHighSalaryAdditionalCostAsync(decimal salary)
    {
        if (salary <= _options.HighSalaryThreshold)
            return 0m;

        if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableHighSalaryCalculation))
            return 0m;

        return salary * _options.HighSalaryAdditionalCostPercentage / MonthsPerYear;
    }

    /// <summary>
    /// Converts monthly cost to per-paycheck deduction
    /// </summary>
    /// <param name="totalMonthlyCost">Total monthly benefit cost</param>
    /// <returns>Deduction amount per paycheck</returns>
    public decimal CalculatePerPaycheckDeduction(decimal totalMonthlyCost)
    {
        if (totalMonthlyCost < 0)
            throw new ArgumentException("Monthly cost cannot be negative", nameof(totalMonthlyCost));

        var totalYearlyCost = totalMonthlyCost * MonthsPerYear;
        return totalYearlyCost / _options.PaychecksPerYear;
    }

    /// <summary>
    /// Calculates age based on date of birth
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Age in years</returns>
    public decimal CalculateAge(DateOnly dateOfBirth)
    {
        var currentDate = DateTime.Today;
        var age = currentDate.Year - dateOfBirth.Year;

        // Adjust if birthday hasn't occurred this year
        if (dateOfBirth > DateOnly.FromDateTime(currentDate.AddYears(-age).Date))
        {
            age--;
        }

        return age;
    }
}
