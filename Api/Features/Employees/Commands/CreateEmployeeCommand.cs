using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Commands;

/// <summary>
/// Command to create a new employee
/// </summary>
public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    decimal Salary,
    DateOnly DateOfBirth
) : IRequest<GetEmployeeDto>;

/// <summary>
/// Handler for creating an employee
/// </summary>
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, GetEmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public CreateEmployeeCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CreateEmployeeCommand for {FirstName} {LastName}", request.FirstName, request.LastName);

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ArgumentException("FirstName is required", nameof(request.FirstName));

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("LastName is required", nameof(request.LastName));

            if (request.Salary <= 0)
                throw new ArgumentException("Salary must be positive", nameof(request.Salary));

            // Create employee entity
            var employee = new Employee
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Salary = request.Salary,
                DateOfBirth = request.DateOfBirth,
                Dependents = new List<Dependent>()
            };

            // Save using Unit of Work
            var createdEmployee = await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var result = MapToDto(createdEmployee);

            _logger.LogInformation("Successfully created employee {EmployeeId}", createdEmployee.Id);
            return result;

        }, cancellationToken);
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
