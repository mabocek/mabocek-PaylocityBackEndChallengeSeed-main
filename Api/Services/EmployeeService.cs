using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;
using Api.Features.Employees.Queries;
using Api.Features.Employees.Commands;
using Api.Features;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Api.Services;

public class EmployeeService : BaseService, IEmployeeService
{
    private readonly IMediator _mediator;
    private readonly IPaycheckCalculationService _paycheckCalculationService;

    public EmployeeService(
        IMediator mediator,
        IFeatureManager featureManager,
        ILogger<EmployeeService> logger,
        IPaycheckCalculationService paycheckCalculationService)
        : base(featureManager, logger)
    {
        _mediator = mediator;
        _paycheckCalculationService = paycheckCalculationService ?? throw new ArgumentNullException(nameof(paycheckCalculationService));
    }

    #region Legacy Methods (for backwards compatibility)

    public async Task<List<GetEmployeeDto>> GetAllAsync()
    {
        return await _mediator.Send(new GetAllEmployeesQuery());
    }

    public async Task<PagedResult<GetEmployeeDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool ascending = true)
    {
        return await _mediator.Send(new GetPagedEmployeesQuery(
            page,
            pageSize,
            sortBy,
            ascending));
    }

    public async Task<GetEmployeeDto?> GetByIdAsync(int id)
    {
        return await _mediator.Send(new GetEmployeeByIdQuery(id));
    }

    public async Task<GetPaycheckDto?> GetPaycheckAsync(int id)
    {
        try
        {
            _logger.LogInformation("Processing paycheck calculation for employee {EmployeeId}", id);

            // Use CQRS to get employee data
            var employee = await _mediator.Send(new GetEmployeePaycheckQuery(id));

            if (employee == null)
            {
                _logger.LogWarning("Employee {EmployeeId} not found for paycheck calculation", id);
                return null;
            }

            // Calculate paycheck using the dedicated service
            var paycheck = await _paycheckCalculationService.CalculatePaycheckAsync(employee);

            _logger.LogInformation("Successfully calculated paycheck for employee {EmployeeId}", id);
            return paycheck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating paycheck for employee {EmployeeId}", id);
            throw;
        }
    }

    public async Task<GetEmployeeDto> CreateAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth)
    {
        return await _mediator.Send(new CreateEmployeeCommand(firstName, lastName, salary, dateOfBirth));
    }

    public async Task<GetEmployeeDto?> UpdateAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth)
    {
        return await _mediator.Send(new UpdateEmployeeCommand(id, firstName, lastName, salary, dateOfBirth));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _mediator.Send(new DeleteEmployeeCommand(id));
    }

    #endregion

    #region New Endpoint-Friendly Methods

    public async Task<ApiResponse<GetEmployeeDto>> GetEmployeeByIdAsync(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var employee = await GetByIdAsync(id);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with id {id} not found");
            }
            return employee;
        }, "retrieving employee");
    }

    public async Task<ApiResponse<PagedResult<GetEmployeeDto>>> GetEmployeesPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortOrder = null)
    {
        return await ExecuteAsync(async () =>
        {
            var ascending = ParseSortOrder(sortOrder);
            return await GetPagedAsync(page, pageSize, sortBy, ascending);
        }, "retrieving employees");
    }

    public async Task<ApiResponse<GetPaycheckDto>> GetEmployeePaycheckAsync(int id)
    {
        // Check feature flag
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnablePaycheckCalculation))
        {
            return Failure<GetPaycheckDto>("Paycheck calculation feature is currently disabled");
        }

        return await ExecuteAsync(async () =>
        {
            var paycheck = await GetPaycheckAsync(id);
            if (paycheck == null)
            {
                throw new ArgumentException($"Employee with id {id} not found");
            }
            return paycheck;
        }, "calculating paycheck");
    }

    public async Task<ApiResponse<GetEmployeeDto>> CreateEmployeeAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth)
    {
        return await ExecuteAsync(async () =>
        {
            return await CreateAsync(firstName, lastName, salary, dateOfBirth);
        }, "creating employee", "Employee created successfully");
    }

    public async Task<ApiResponse<GetEmployeeDto>> UpdateEmployeeAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth)
    {
        return await ExecuteAsync(async () =>
        {
            var employee = await UpdateAsync(id, firstName, lastName, salary, dateOfBirth);
            if (employee == null)
            {
                throw new ArgumentException($"Employee with id {id} not found");
            }
            return employee;
        }, "updating employee", "Employee updated successfully");
    }

    public async Task<ApiResponse<object>> DeleteEmployeeAsync(int id)
    {
        return await ExecuteAsync(async () =>
        {
            var success = await DeleteAsync(id);
            if (!success)
            {
                throw new ArgumentException($"Employee with id {id} not found");
            }
            return new object();
        }, "deleting employee", "Employee deleted successfully");
    }

    #endregion
}
