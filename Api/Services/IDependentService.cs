using Api.Dtos.Dependent;
using Api.Models;

namespace Api.Services;

/// <summary>
/// Service interface for managing dependent-related operations.
/// </summary>
public interface IDependentService
{
    /// <summary>
    /// Retrieves a paged list of dependents with optional filtering and sorting.
    /// </summary>s
    Task<PagedResult<GetDependentDto>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true);

    /// <summary>
    /// Retrieves a specific dependent by their ID.
    /// </summary>
    Task<GetDependentDto?> GetByIdAsync(int id);


    /// <summary>
    /// Alternative method to retrieve a dependent by their ID.
    /// </summary>
    Task<GetDependentDto?> GetDependentByIdAsync(int id);

    /// <summary>
    /// Retrieves a paged list of dependents with string-based filtering and sorting options.
    /// </summary>
    Task<PagedResult<GetDependentDto>> GetDependentsPagedAsync(
        int page = 1,
        int pageSize = 10,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null);
}
