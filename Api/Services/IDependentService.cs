using Api.Dtos.Dependent;
using Api.Models;

namespace Api.Services;

public interface IDependentService
{
    Task<PagedResult<GetDependentDto>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true);

    Task<GetDependentDto?> GetByIdAsync(int id);

    Task<GetDependentDto?> GetDependentByIdAsync(int id);

    Task<PagedResult<GetDependentDto>> GetDependentsPagedAsync(
        int page = 1,
        int pageSize = 10,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null);
}
