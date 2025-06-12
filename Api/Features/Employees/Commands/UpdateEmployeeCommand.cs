using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Commands;

/// <summary>
/// Command to update an existing employee
/// </summary>
public record UpdateEmployeeCommand(
    int Id,
    string FirstName,
    string LastName,
    decimal Salary,
    DateOnly DateOfBirth
) : IRequest<GetEmployeeDto?>;

/// <summary>
/// Handler for updating an employee
/// </summary>
public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, GetEmployeeDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateEmployeeCommandHandler> _logger;

    public UpdateEmployeeCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeDto?> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing UpdateEmployeeCommand for employee {EmployeeId}", request.Id);

            // Validate input
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ArgumentException("FirstName is required", nameof(request.FirstName));

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("LastName is required", nameof(request.LastName));

            if (request.Salary <= 0)
                throw new ArgumentException("Salary must be positive", nameof(request.Salary));

            // Execute within transaction for data consistency
            var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Get existing employee using repository through Unit of Work
                var employee = await _unitOfWork.Employees.GetWithDependentsAsync(request.Id, cancellationToken);
                if (employee == null)
                {
                    _logger.LogWarning("Employee with id {EmployeeId} not found for update", request.Id);
                    return null;
                }

                // Update employee properties
                employee.FirstName = request.FirstName.Trim();
                employee.LastName = request.LastName.Trim();
                employee.Salary = request.Salary;
                employee.DateOfBirth = request.DateOfBirth;

                // Update using repository through Unit of Work
                await _unitOfWork.Employees.UpdateAsync(employee, cancellationToken);

                // Save changes within transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return MapToDto(employee);
            }, cancellationToken);

            _logger.LogInformation("Successfully updated employee {EmployeeId}", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing UpdateEmployeeCommand for employee {EmployeeId}", request.Id);
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
