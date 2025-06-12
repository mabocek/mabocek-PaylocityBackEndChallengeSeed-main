using MediatR;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Employees.Queries;

/// <summary>
/// Query to get paginated employees
/// </summary>
public record GetPagedEmployeesQuery(
    int Page,
    int PageSize,
    string? SortBy = null,
    bool Ascending = true) : IRequest<PagedResult<GetEmployeeDto>>;

/// <summary>
/// Handler for getting paginated employees
/// </summary>
public class GetPagedEmployeesQueryHandler : IRequestHandler<GetPagedEmployeesQuery, PagedResult<GetEmployeeDto>>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<GetPagedEmployeesQueryHandler> _logger;

    public GetPagedEmployeesQueryHandler(IEmployeeRepository employeeRepository, ILogger<GetPagedEmployeesQueryHandler> logger)
    {
        _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<GetEmployeeDto>> Handle(GetPagedEmployeesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetPagedEmployeesQuery - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, Ascending: {Ascending}",
                request.Page, request.PageSize, request.SortBy, request.Ascending);

            var pagedEmployees = await _employeeRepository.GetPagedWithDependentsAsync(
                request.Page,
                request.PageSize,
                request.SortBy,
                request.Ascending,
                cancellationToken);

            var result = new PagedResult<GetEmployeeDto>(
                pagedEmployees.Items.Select(MapToDto).ToList(),
                pagedEmployees.TotalItems,
                pagedEmployees.CurrentPage,
                pagedEmployees.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} employees out of {TotalItems} total",
                result.Items.Count, result.TotalItems);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetPagedEmployeesQuery");
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
