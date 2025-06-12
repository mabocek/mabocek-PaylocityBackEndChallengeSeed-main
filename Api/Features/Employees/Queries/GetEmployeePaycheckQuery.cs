using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;
using Microsoft.Extensions.Logging;

namespace Api.Features.Employees.Queries;

/// <summary>
/// Query to get employee with dependents by id for paycheck calculation
/// </summary>
public record GetEmployeePaycheckQuery(int EmployeeId) : IRequest<GetEmployeeDto?>;

/// <summary>
/// Handler for getting employee with dependents - data access only
/// </summary>
public class GetEmployeePaycheckQueryHandler : IRequestHandler<GetEmployeePaycheckQuery, GetEmployeeDto?>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<GetEmployeePaycheckQueryHandler> _logger;

    public GetEmployeePaycheckQueryHandler(
        IEmployeeRepository employeeRepository,
        ILogger<GetEmployeePaycheckQueryHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeDto?> Handle(GetEmployeePaycheckQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing GetEmployeePaycheckQuery for employee {EmployeeId}", request.EmployeeId);

        var employee = await _employeeRepository.GetWithDependentsAsync(request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            _logger.LogWarning("Employee with id {EmployeeId} not found", request.EmployeeId);
            return null;
        }

        var employeeDto = MapToEmployeeDto(employee);
        _logger.LogInformation("Successfully retrieved employee {EmployeeId} with dependents", request.EmployeeId);

        return employeeDto;
    }

    private static GetEmployeeDto MapToEmployeeDto(Employee employee)
    {
        return new GetEmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Salary,
            employee.DateOfBirth,
            employee.Dependents.Select(d => new GetDependentDto(
                d.Id,
                d.FirstName,
                d.LastName,
                d.DateOfBirth,
                d.Relationship,
                d.EmployeeId
            )).ToList()
        );
    }
}
