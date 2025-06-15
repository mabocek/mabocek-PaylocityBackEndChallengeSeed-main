namespace Api.Dtos.Paycheck;

/// <summary>
/// DTO representing a calculated paycheck for an employee.
/// Based on requirements: "able to calculate and view a paycheck for an employee"
/// </summary>
public record GetPaycheckDto(
    /// <summary>
    /// The ID of the employee this paycheck belongs to
    /// </summary>
    int EmployeeId,

    /// <summary>
    /// Full name of the employee for display purposes
    /// </summary>
    string EmployeeName,

    /// <summary>
    /// Employee's gross pay per paycheck (annual salary / 26 paychecks per year)
    /// Based on requirement: "26 paychecks per year"
    /// </summary>
    decimal GrossPay,

    /// <summary>
    /// Total benefit deductions for this paycheck
    /// Calculated based on monthly benefit costs spread evenly across 26 paychecks
    /// Based on requirement: "deductions spread as evenly as possible on each paycheck"
    /// </summary>
    decimal BenefitDeductions,

    /// <summary>
    /// Net pay after benefit deductions (GrossPay - BenefitDeductions)
    /// </summary>
    decimal NetPay,

    /// <summary>
    /// Detailed breakdown of how the benefit deductions were calculated
    /// </summary>
    PaycheckDetailsDto Details
);
