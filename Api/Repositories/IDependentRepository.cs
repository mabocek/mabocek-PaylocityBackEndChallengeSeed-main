using Api.Models;

namespace Api.Repositories;

/// <summary>
/// Dependent-specific repository interface extending generic repository
/// </summary>
public interface IDependentRepository : IRepository<Dependent>
{
    /// <summary>
    /// Gets dependents by employee id
    /// </summary>
    Task<IEnumerable<Dependent>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dependents by relationship type
    /// </summary>
    Task<IEnumerable<Dependent>> GetByRelationshipAsync(Relationship relationship, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated dependents with filtering and sorting
    /// </summary>
    Task<PagedResult<Dependent>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dependents count by employee id
    /// </summary>
    Task<int> GetCountByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default);
}
