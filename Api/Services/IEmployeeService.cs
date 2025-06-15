using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;

namespace Api.Services;

/// <summary>
/// Service interface for managing employee operations including CRUD operations, pagination, and paycheck calculations.
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Retrieves a paginated list of employees with optional sorting.
    /// </summary>
    Task<PagedResult<GetEmployeeDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    Task<GetEmployeeDto?> GetByIdAsync(int id);

    /// <summary>
    /// Calculates and retrieves the paycheck information for an employee.
    /// </summary>
    Task<GetPaycheckDto?> GetPaycheckAsync(int id);

    /// <summary>
    /// Creates a new employee with the specified details.
    /// </summary>
    Task<GetEmployeeDto> CreateAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    /// <summary>
    /// Updates an existing employee's information.
    /// </summary>
    Task<GetEmployeeDto?> UpdateAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    /// <summary>
    /// Deletes an employee by their unique identifier.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    Task<GetEmployeeDto?> GetEmployeeByIdAsync(int id);

    /// <summary>
    /// Retrieves a paginated list of employees with optional sorting and ordering.
    /// </summary>
    Task<PagedResult<GetEmployeeDto>> GetEmployeesPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortOrder = null);

    /// <summary>
    /// Calculates and retrieves the paycheck information for an employee.
    /// </summary>
    Task<GetPaycheckDto?> GetEmployeePaycheckAsync(int id);

    /// <summary>
    /// Creates a new employee with the specified details.
    /// </summary>
    Task<GetEmployeeDto> CreateEmployeeAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    /// <summary>
    /// Updates an existing employee's information.
    /// </summary>
    Task<GetEmployeeDto> UpdateEmployeeAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    /// <summary>
    /// Deletes an employee by their unique identifier.
    /// </summary>
    Task DeleteEmployeeAsync(int id);
}
