using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Queries;

/// <summary>
/// Query to get all employees
/// </summary>
public record GetAllEmployeesQuery : IRequest<List<GetEmployeeDto>>;

/// <summary>
/// Handler for getting all employees
/// </summary>
public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, List<GetEmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<GetAllEmployeesQueryHandler> _logger;

    public GetAllEmployeesQueryHandler(IEmployeeRepository employeeRepository, ILogger<GetAllEmployeesQueryHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<GetEmployeeDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetAllEmployeesQuery");

            var employees = await _employeeRepository.GetAllWithDependentsAsync(cancellationToken);

            var result = employees.Select(MapToDto).ToList();

            _logger.LogInformation("Successfully retrieved {Count} employees", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllEmployeesQuery");
            throw;
        }
    }

    private static GetEmployeeDto MapToDto(Employee employee)
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
