using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;

namespace Api.Services;

public interface IEmployeeService
{
    Task<PagedResult<GetEmployeeDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool ascending = true);

    Task<GetEmployeeDto?> GetByIdAsync(int id);
    Task<GetPaycheckDto?> GetPaycheckAsync(int id);
    Task<GetEmployeeDto> CreateAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth);
    Task<GetEmployeeDto?> UpdateAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth);
    Task<bool> DeleteAsync(int id);
    Task<GetEmployeeDto?> GetEmployeeByIdAsync(int id);

    Task<PagedResult<GetEmployeeDto>> GetEmployeesPagedAsync(
        int page = 1,
        int pageSize = 10,
        string? sortBy = null,
        string? sortOrder = null);

    Task<GetPaycheckDto?> GetEmployeePaycheckAsync(int id);

    Task<GetEmployeeDto> CreateEmployeeAsync(string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    Task<GetEmployeeDto> UpdateEmployeeAsync(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth);

    Task DeleteEmployeeAsync(int id);
}
