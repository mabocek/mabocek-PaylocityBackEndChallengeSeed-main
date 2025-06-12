using Api.Models;

namespace Api.Repositories;

/// <summary>
/// Employee-specific repository interface extending generic repository
/// </summary>
public interface IEmployeeRepository : IRepository<Employee>
{
    /// <summary>
    /// Gets employee with dependents by id
    /// </summary>
    Task<Employee?> GetWithDependentsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all employees with their dependents
    /// </summary>
    Task<IEnumerable<Employee>> GetAllWithDependentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated employees with their dependents
    /// </summary>
    Task<PagedResult<Employee>> GetPagedWithDependentsAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets employees by salary range
    /// </summary>
    Task<IEnumerable<Employee>> GetBySalaryRangeAsync(decimal minSalary, decimal maxSalary, CancellationToken cancellationToken = default);
}
