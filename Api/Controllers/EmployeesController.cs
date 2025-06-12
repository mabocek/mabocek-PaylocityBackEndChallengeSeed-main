/*
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController : ControllerBase
{
    [SwaggerOperation(Summary = "Get employee by id")]
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetEmployeeDto>>> Get(int id)
    {
        // Simulate async operation
        await Task.Delay(1);

        // Get all employees first (reusing the logic from GetAll)
        var employees = await GetAllEmployeesData();

        // Find the employee by id
        var employee = employees.FirstOrDefault(e => e.Id == id);

        if (employee == null)
        {
            return NotFound(new ApiResponse<GetEmployeeDto>
            {
                Data = null,
                Success = false,
                Message = $"Employee with id {id} not found"
            });
        }

        var result = new ApiResponse<GetEmployeeDto>
        {
            Data = employee,
            Success = true
        };

        return Ok(result);
    }

    [SwaggerOperation(Summary = "Get all employees")]
    [HttpGet("")]
    public async Task<ActionResult<ApiResponse<List<GetEmployeeDto>>>> GetAll()
    {
        // Simulate async operation
        await Task.Delay(1);

        var employees = await GetAllEmployeesData();

        var result = new ApiResponse<List<GetEmployeeDto>>
        {
            Data = employees,
            Success = true
        };

        return result;
    }

    [SwaggerOperation(Summary = "Get paycheck calculation for employee")]
    [HttpGet("{id}/paycheck")]
    public async Task<ActionResult<ApiResponse<GetPaycheckDto>>> GetPaycheck(int id)
    {
        // Simulate async operation
        await Task.Delay(1);

        // Get all employees first (reusing the logic from GetAll)
        var employees = await GetAllEmployeesData();

        // Find the employee by id
        var employee = employees.FirstOrDefault(e => e.Id == id);

        if (employee == null)
        {
            return NotFound(new ApiResponse<GetPaycheckDto>
            {
                Data = null,
                Success = false,
                Message = $"Employee with id {id} not found"
            });
        }

        // Calculate paycheck based on requirements
        var paycheck = await CalculatePaycheck(employee);

        var result = new ApiResponse<GetPaycheckDto>
        {
            Data = paycheck,
            Success = true
        };

        return Ok(result);
    }

    private async Task<List<GetEmployeeDto>> GetAllEmployeesData()
    {
        // Simulate async operation
        await Task.Delay(1);

        //task: use a more realistic production approach
        var employees = new List<GetEmployeeDto>
        {
            new()
            {
                Id = 1,
                FirstName = "LeBron",
                LastName = "James",
                Salary = 75420.99m,
                DateOfBirth = new DateOnly(1984, 12, 30)
            },
            new()
            {
                Id = 2,
                FirstName = "Ja",
                LastName = "Morant",
                Salary = 92365.22m,
                DateOfBirth = new DateOnly(1999, 8, 10),
                Dependents = new List<GetDependentDto>
                {
                    new()
                    {
                        Id = 1,
                        FirstName = "Spouse",
                        LastName = "Morant",
                        Relationship = Relationship.Spouse,
                        DateOfBirth = new DateOnly(1998, 3, 3)
                    },
                    new()
                    {
                        Id = 2,
                        FirstName = "Child1",
                        LastName = "Morant",
                        Relationship = Relationship.Child,
                        DateOfBirth = new DateOnly(2020, 6, 23)
                    },
                    new()
                    {
                        Id = 3,
                        FirstName = "Child2",
                        LastName = "Morant",
                        Relationship = Relationship.Child,
                        DateOfBirth = new DateOnly(2021, 5, 18)
                    }
                }
            },
            new()
            {
                Id = 3,
                FirstName = "Michael",
                LastName = "Jordan",
                Salary = 143211.12m,
                DateOfBirth = new DateOnly(1963, 2, 17),
                Dependents = new List<GetDependentDto>
                {
                    new()
                    {
                        Id = 4,
                        FirstName = "DP",
                        LastName = "Jordan",
                        Relationship = Relationship.DomesticPartner,
                        DateOfBirth = new DateOnly(1974, 1, 2)
                    }
                }
            }
        };

        return employees;
    }

    /// <summary>
    /// Calculates paycheck for an employee based on business requirements.
    /// Implementation follows all benefit calculation rules from requirements.
    /// </summary>
    /// <param name="employee">The employee to calculate paycheck for</param>
    /// <returns>Calculated paycheck with detailed breakdown</returns>
    private async Task<GetPaycheckDto> CalculatePaycheck(GetEmployeeDto employee)
    {
        // Simulate async operation
        await Task.Delay(1);

        // Calculate gross pay per paycheck
        // Based on requirement: "26 paychecks per year"
        const int paychecksPerYear = 26;
        var grossPayPerPaycheck = employee.Salary / paychecksPerYear;

        // Calculate benefit costs following business rules
        var details = CalculateBenefitDetails(employee);

        var paycheck = new GetPaycheckDto
        {
            EmployeeId = employee.Id,
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            GrossPay = grossPayPerPaycheck,
            BenefitDeductions = details.PerPaycheckDeduction,
            NetPay = grossPayPerPaycheck - details.PerPaycheckDeduction,
            Details = details
        };

        return paycheck;
    }

    /// <summary>
    /// Calculates detailed benefit cost breakdown based on requirements.
    /// Implements all business rules for benefit cost calculations.
    /// </summary>
    /// <param name="employee">The employee to calculate benefits for</param>
    /// <returns>Detailed benefit calculation breakdown</returns>
    private PaycheckDetailsDto CalculateBenefitDetails(GetEmployeeDto employee)
    {
        const decimal employeeBaseMonthlyCost = 1000m; // "$1,000 per month (for benefits)"
        const decimal dependentBaseMonthlyCost = 600m; // "$600 cost per month (for benefits)"
        const decimal seniorAdditionalMonthlyCost = 200m; // "$200 per month" for dependents over 50
        const decimal highSalaryCostPercentage = 0.02m; // "2% of their yearly salary"
        const decimal highSalaryThreshold = 80000m; // "more than $80,000 per year"
        const int paychecksPerYear = 26;

        var details = new PaycheckDetailsDto
        {
            EmployeeBaseCost = employeeBaseMonthlyCost,
            PaychecksPerYear = paychecksPerYear
        };

        // Calculate dependent costs and breakdowns
        decimal totalDependentsCost = 0m;
        decimal totalSeniorCost = 0m;
        var dependentBreakdowns = new List<DependentCostBreakdownDto>();

        foreach (var dependent in employee.Dependents)
        {
            var age = CalculateAge(dependent.DateOfBirth);
            var baseCost = dependentBaseMonthlyCost;
            var seniorCost = age > 50 ? seniorAdditionalMonthlyCost : 0m;
            var totalCost = baseCost + seniorCost;

            totalDependentsCost += baseCost;
            totalSeniorCost += seniorCost;

            dependentBreakdowns.Add(new DependentCostBreakdownDto
            {
                DependentId = dependent.Id,
                DependentName = $"{dependent.FirstName} {dependent.LastName}",
                Relationship = dependent.Relationship,
                Age = age,
                BaseCost = baseCost,
                SeniorAdditionalCost = seniorCost,
                TotalCost = totalCost
            });
        }

        details.DependentsCost = totalDependentsCost;
        details.SeniorDependentsCost = totalSeniorCost;
        details.DependentBreakdowns = dependentBreakdowns;

        // Calculate high salary additional cost
        // "employees that make more than $80,000 per year will incur an additional 2% of their yearly salary"
        details.HighSalaryAdditionalCost = employee.Salary > highSalaryThreshold
            ? (employee.Salary * highSalaryCostPercentage) / 12m // Convert yearly to monthly
            : 0m;

        // Calculate total monthly cost
        details.TotalMonthlyCost = details.EmployeeBaseCost +
                                  details.DependentsCost +
                                  details.SeniorDependentsCost +
                                  details.HighSalaryAdditionalCost;

        // Calculate per-paycheck deduction
        // "deductions spread as evenly as possible on each paycheck"
        // Convert monthly to yearly, then divide by paychecks per year
        details.PerPaycheckDeduction = (details.TotalMonthlyCost * 12m) / paychecksPerYear;

        return details;
    }

    /// <summary>
    /// Calculates age from date of birth as of current date.
    /// Used to determine eligibility for over-50 dependent additional costs.
    /// </summary>
    /// <param name="dateOfBirth">Date of birth</param>
    /// <returns>Current age in years</returns>
    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;

        // Subtract one year if birthday hasn't occurred this year
        if (dateOfBirth > today.AddYears(-age))
            age--;

        return age;
    }
}
*/
