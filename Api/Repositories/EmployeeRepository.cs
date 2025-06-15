using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;
using Api.Constants;

namespace Api.Repositories;

/// <summary>
/// Employee repository implementation with specific employee operations
/// Implements performance optimizations and relationship loading
/// </summary>
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(ApplicationDbContext context, ILogger<EmployeeRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<Employee?> GetWithDependentsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Dependents)
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee with dependents for id {EmployeeId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<Employee>> GetAllWithDependentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(e => e.Dependents)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all employees with dependents");
            throw;
        }
    }

    public async Task<PagedResult<Employee>> GetPagedWithDependentsAsync(
        int page,
        int pageSize,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting paginated employees - Page: {Page}, PageSize: {PageSize}, SortBy: {SortBy}, Ascending: {Ascending}",
                page, pageSize, sortBy, ascending);

            var query = _dbSet.Include(e => e.Dependents).AsQueryable();

            // Apply sorting at database level for better performance
            query = ApplySorting(query, sortBy, ascending);

            // Get total count before pagination
            var totalItems = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {ItemCount} employees out of {TotalItems} total", items.Count, totalItems);

            return new PagedResult<Employee>(items, totalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated employees");
            throw;
        }
    }

    /// <summary>
    /// Applies sorting to the query based on the specified criteria
    /// </summary>
    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> query, string? sortBy, bool ascending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return ascending ? query.OrderBy(e => e.Id) : query.OrderByDescending(e => e.Id);
        }

        return sortBy.ToLower() switch
        {
            SortFieldsConstants.Employee.FirstName => ascending ? query.OrderBy(e => e.FirstName) : query.OrderByDescending(e => e.FirstName),
            SortFieldsConstants.Employee.LastName => ascending ? query.OrderBy(e => e.LastName) : query.OrderByDescending(e => e.LastName),
            SortFieldsConstants.Employee.Salary => ascending ? query.OrderBy(e => e.Salary) : query.OrderByDescending(e => e.Salary),
            SortFieldsConstants.Employee.DateOfBirth => ascending ? query.OrderBy(e => e.DateOfBirth) : query.OrderByDescending(e => e.DateOfBirth),
            _ => ascending ? query.OrderBy(e => e.Id) : query.OrderByDescending(e => e.Id)
        };
    }

    public async Task<IEnumerable<Employee>> GetBySalaryRangeAsync(decimal minSalary, decimal maxSalary, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(e => e.Salary >= minSalary && e.Salary <= maxSalary)
                .Include(e => e.Dependents)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employees by salary range {MinSalary}-{MaxSalary}", minSalary, maxSalary);
            throw;
        }
    }
}
