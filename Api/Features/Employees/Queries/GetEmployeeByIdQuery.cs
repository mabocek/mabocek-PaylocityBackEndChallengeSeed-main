using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Queries;

/// <summary>
/// Query to get employee by id
/// </summary>
public record GetEmployeeByIdQuery(int Id) : IRequest<GetEmployeeDto?>;

/// <summary>
/// Handler for getting employee by id
/// </summary>
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, GetEmployeeDto?>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<GetEmployeeByIdQueryHandler> _logger;

    public GetEmployeeByIdQueryHandler(IEmployeeRepository employeeRepository, ILogger<GetEmployeeByIdQueryHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeDto?> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetEmployeeByIdQuery for employee {EmployeeId}", request.Id);

            var employee = await _employeeRepository.GetWithDependentsAsync(request.Id, cancellationToken);

            if (employee == null)
            {
                _logger.LogWarning("Employee with id {EmployeeId} not found", request.Id);
                return null;
            }

            var result = MapToDto(employee);
            _logger.LogInformation("Successfully retrieved employee {EmployeeId}", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetEmployeeByIdQuery for employee {EmployeeId}", request.Id);
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
