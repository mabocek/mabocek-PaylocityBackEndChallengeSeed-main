using Api.Dtos.Employee;
using Api.Dtos.Paycheck;
using Api.Models;

namespace Api.Services;

public interface IEmployeeService
{
    Task<List<GetEmployeeDto>> GetAllAsync();

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
}
