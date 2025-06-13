using Microsoft.EntityFrameworkCore;
using Api.Data;
using Api.Models;

namespace Api.Repositories;

/// <summary>
/// Dependent repository implementation with specific dependent operations
/// </summary>
public class DependentRepository : Repository<Dependent>, IDependentRepository
{
    private const string Firstname = "firstname";
    private const string Lastname = "lastname";
    private const string Dateofbirth = "dateofbirth";
    private const string Relationship = "relationship";
    private const string Employeeid = "employeeid";

    public DependentRepository(ApplicationDbContext context, ILogger<DependentRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IEnumerable<Dependent>> GetByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(d => d.EmployeeId == employeeId)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependents for employee {EmployeeId}", employeeId);
            throw;
        }
    }

    public async Task<IEnumerable<Dependent>> GetByRelationshipAsync(Relationship relationship, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(d => d.Relationship == relationship)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependents by relationship {Relationship}", relationship);
            throw;
        }
    }

    public async Task<PagedResult<Dependent>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting paginated dependents - Page: {Page}, PageSize: {PageSize}, EmployeeId: {EmployeeId}, Relationship: {Relationship}, SortBy: {SortBy}, Ascending: {Ascending}",
                page, pageSize, employeeId, relationship, sortBy, ascending);

            var query = _dbSet.AsQueryable();

            // Apply filtering at database level for better performance
            if (employeeId.HasValue)
            {
                query = query.Where(d => d.EmployeeId == employeeId.Value);
            }

            if (relationship.HasValue)
            {
                query = query.Where(d => d.Relationship == relationship.Value);
            }

            // Apply sorting at database level
            query = ApplySorting(query, sortBy, ascending);

            // Get total count before pagination
            var totalItems = await query.CountAsync(cancellationToken);

            // Apply pagination
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {ItemCount} dependents out of {TotalItems} total", items.Count, totalItems);

            return new PagedResult<Dependent>(items, totalItems, page, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paginated dependents");
            throw;
        }
    }

    /// <summary>
    /// Applies sorting to the query based on the specified criteria
    /// </summary>
    private static IQueryable<Dependent> ApplySorting(IQueryable<Dependent> query, string? sortBy, bool ascending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return ascending ? query.OrderBy(d => d.Id) : query.OrderByDescending(d => d.Id);
        }

        return sortBy.ToLower() switch
        {
            Firstname => ascending ? query.OrderBy(d => d.FirstName) : query.OrderByDescending(d => d.FirstName),
            Lastname => ascending ? query.OrderBy(d => d.LastName) : query.OrderByDescending(d => d.LastName),
            Dateofbirth => ascending ? query.OrderBy(d => d.DateOfBirth) : query.OrderByDescending(d => d.DateOfBirth),
            Relationship => ascending ? query.OrderBy(d => d.Relationship) : query.OrderByDescending(d => d.Relationship),
            Employeeid => ascending ? query.OrderBy(d => d.EmployeeId) : query.OrderByDescending(d => d.EmployeeId),
            _ => ascending ? query.OrderBy(d => d.Id) : query.OrderByDescending(d => d.Id)
        };
    }

    public async Task<int> GetCountByEmployeeIdAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Where(d => d.EmployeeId == employeeId)
                .CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dependent count for employee {EmployeeId}", employeeId);
            throw;
        }
    }
}
