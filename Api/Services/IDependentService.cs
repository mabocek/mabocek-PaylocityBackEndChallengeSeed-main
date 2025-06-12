using Api.Dtos.Dependent;
using Api.Models;

namespace Api.Services;

public interface IDependentService
{
    Task<List<GetDependentDto>> GetAllAsync(
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true);

    Task<PagedResult<GetDependentDto>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true);

    Task<GetDependentDto?> GetByIdAsync(int id);
}
