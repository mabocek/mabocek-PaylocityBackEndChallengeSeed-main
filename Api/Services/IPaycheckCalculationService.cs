using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;

namespace Api.Services;

/// <summary>
/// Interface for paycheck calculation service
/// </summary>
public interface IPaycheckCalculationService
{
    /// <summary>
    /// Calculates paycheck for an employee based on business requirements
    /// </summary>
    Task<GetPaycheckDto> CalculatePaycheckAsync(GetEmployeeDto employee);

    /// <summary>
    /// Calculates gross pay per paycheck based on annual salary
    /// </summary>
    decimal CalculateGrossPayPerPaycheck(decimal annualSalary);

    /// <summary>
    /// Calculates benefit cost details for an employee
    /// </summary>
    Task<PaycheckDetailsDto> CalculateBenefitDetailsAsync(GetEmployeeDto employee);

    /// <summary>
    /// Calculates cost breakdown for a single dependent
    /// </summary>
    Task<DependentCostBreakdownDto> CalculateDependentCostAsync(GetDependentDto dependent);

    /// <summary>
    /// Calculates additional cost for high salary employees
    /// </summary>
    Task<decimal> CalculateHighSalaryAdditionalCostAsync(decimal salary);

    /// <summary>
    /// Converts monthly cost to per-paycheck deduction
    /// </summary>
    decimal CalculatePerPaycheckDeduction(decimal totalMonthlyCost);

    /// <summary>
    /// Calculates age based on date of birth
    /// </summary>
    decimal CalculateAge(DateOnly dateOfBirth);
}
