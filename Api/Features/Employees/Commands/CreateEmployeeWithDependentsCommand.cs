using MediatR;
using Api.Dtos.Employee;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Commands;

/// <summary>
/// Command to create a new employee with dependents using Unit of Work pattern
/// </summary>
public record CreateEmployeeWithDependentsCommand(
    string FirstName,
    string LastName,
    decimal Salary,
    DateOnly DateOfBirth,
    List<CreateDependentDto> Dependents
) : IRequest<GetEmployeeDto>;

/// <summary>
/// DTO for creating dependents
/// </summary>
public record CreateDependentDto(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    Relationship Relationship
);

/// <summary>
/// Handler for creating an employee with dependents using Unit of Work pattern
/// Demonstrates transactional operations across multiple entities
/// </summary>
public class CreateEmployeeWithDependentsCommandHandler : IRequestHandler<CreateEmployeeWithDependentsCommand, GetEmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateEmployeeWithDependentsCommandHandler> _logger;

    public CreateEmployeeWithDependentsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateEmployeeWithDependentsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetEmployeeDto> Handle(CreateEmployeeWithDependentsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing CreateEmployeeWithDependentsCommand for {FirstName} {LastName} with {DependentCount} dependents",
            request.FirstName, request.LastName, request.Dependents.Count);

        // Use Unit of Work to ensure all operations are performed in a single transaction
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Validate input
            ValidateRequest(request);

            // Create employee entity
            var employee = new Employee
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Salary = request.Salary,
                DateOfBirth = request.DateOfBirth
            };

            // Add employee using the Unit of Work's repository
            var createdEmployee = await _unitOfWork.Employees.AddAsync(employee, cancellationToken);

            // Save changes to get the employee ID
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Create dependents if any
            var dependentDtos = new List<Api.Dtos.Dependent.GetDependentDto>();

            if (request.Dependents.Any())
            {
                foreach (var dependentRequest in request.Dependents)
                {
                    var dependent = new Dependent
                    {
                        FirstName = dependentRequest.FirstName,
                        LastName = dependentRequest.LastName,
                        DateOfBirth = dependentRequest.DateOfBirth,
                        Relationship = dependentRequest.Relationship,
                        EmployeeId = createdEmployee.Id
                    };

                    var createdDependent = await _unitOfWork.Dependents.AddAsync(dependent, cancellationToken);

                    dependentDtos.Add(new Api.Dtos.Dependent.GetDependentDto(
                        createdDependent.Id,
                        createdDependent.FirstName,
                        createdDependent.LastName,
                        createdDependent.DateOfBirth,
                        createdDependent.Relationship,
                        createdDependent.EmployeeId
                    ));
                }

                // Save all dependents
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Successfully created employee {EmployeeId} with {DependentCount} dependents",
                createdEmployee.Id, dependentDtos.Count);

            // Return the complete employee DTO
            return new GetEmployeeDto(
                createdEmployee.Id,
                createdEmployee.FirstName,
                createdEmployee.LastName,
                createdEmployee.Salary,
                createdEmployee.DateOfBirth,
                dependentDtos
            );

        }, cancellationToken);
    }

    private void ValidateRequest(CreateEmployeeWithDependentsCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("FirstName is required", nameof(request.FirstName));

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("LastName is required", nameof(request.LastName));

        if (request.Salary <= 0)
            throw new ArgumentException("Salary must be positive", nameof(request.Salary));

        if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("DateOfBirth cannot be in the future", nameof(request.DateOfBirth));

        // Validate dependents
        foreach (var dependent in request.Dependents)
        {
            if (string.IsNullOrWhiteSpace(dependent.FirstName))
                throw new ArgumentException("Dependent FirstName is required");

            if (string.IsNullOrWhiteSpace(dependent.LastName))
                throw new ArgumentException("Dependent LastName is required");

            if (dependent.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
                throw new ArgumentException("Dependent DateOfBirth cannot be in the future");
        }

        // Validate business rule: employee may only have 1 spouse or domestic partner (not both)
        ValidateSpouseAndDomesticPartnerConstraint(request.Dependents);
    }

    /// <summary>
    /// Validates that an employee may only have 1 spouse or domestic partner (not both)
    /// </summary>
    /// <param name="dependents">List of dependents to validate</param>
    /// <exception cref="ArgumentException">Thrown when employee has both spouse and domestic partner</exception>
    private static void ValidateSpouseAndDomesticPartnerConstraint(List<CreateDependentDto> dependents)
    {
        var hasSpouse = dependents.Any(d => d.Relationship == Relationship.Spouse);
        var hasDomesticPartner = dependents.Any(d => d.Relationship == Relationship.DomesticPartner);

        if (hasSpouse && hasDomesticPartner)
        {
            throw new ArgumentException("An employee may only have 1 spouse or domestic partner, not both");
        }

        // Validate that there's only one spouse
        var spouseCount = dependents.Count(d => d.Relationship == Relationship.Spouse);
        if (spouseCount > 1)
        {
            throw new ArgumentException("An employee may only have 1 spouse");
        }

        // Validate that there's only one domestic partner
        var domesticPartnerCount = dependents.Count(d => d.Relationship == Relationship.DomesticPartner);
        if (domesticPartnerCount > 1)
        {
            throw new ArgumentException("An employee may only have 1 domestic partner");
        }
    }
}
