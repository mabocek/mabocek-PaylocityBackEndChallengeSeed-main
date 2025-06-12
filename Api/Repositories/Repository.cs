using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Api.Data;

namespace Api.Repositories;

/// <summary>
/// Generic repository implementation providing common data access operations
/// Implements error handling, logging, and performance optimizations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    protected readonly ILogger<Repository<T>> _logger;

    public Repository(ApplicationDbContext context, ILogger<Repository<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting entity {EntityType} with id {Id}", typeof(T).Name, id);
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting entity {EntityType} with id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting all entities of type {EntityType}", typeof(T).Name);
            return await _dbSet.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all entities of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Finding entities of type {EntityType} with predicate", typeof(T).Name);
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding entities of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Getting first entity of type {EntityType} with predicate", typeof(T).Name);
            return await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Adding entity of type {EntityType}", typeof(T).Name);
            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            _logger.LogInformation("Successfully staged entity of type {EntityType} for addition", typeof(T).Name);
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Updating entity of type {EntityType}", typeof(T).Name);
            _dbSet.Update(entity);
            _logger.LogInformation("Successfully staged entity of type {EntityType} for update", typeof(T).Name);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting entity of type {EntityType}", typeof(T).Name);
            _dbSet.Remove(entity);
            _logger.LogInformation("Successfully staged entity of type {EntityType} for deletion", typeof(T).Name);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity of type {EntityType}", typeof(T).Name);
            throw;
        }
    }

    public virtual async Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Deleting entity {EntityType} with id {Id}", typeof(T).Name, id);
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity != null)
            {
                await DeleteAsync(entity, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {EntityType} with id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking if entity {EntityType} with id {Id} exists", typeof(T).Name, id);
            return await GetByIdAsync(id, cancellationToken) != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of entity {EntityType} with id {Id}", typeof(T).Name, id);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Counting entities of type {EntityType}", typeof(T).Name);
            return predicate == null
                ? await _dbSet.CountAsync(cancellationToken)
                : await _dbSet.CountAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting entities of type {EntityType}", typeof(T).Name);
            throw;
        }
    }
}
