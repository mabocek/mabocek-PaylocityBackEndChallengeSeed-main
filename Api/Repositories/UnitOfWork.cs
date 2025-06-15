using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Api.Data;

namespace Api.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions and repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IEmployeeRepository _employees;
    private readonly IDependentRepository _dependents;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    public UnitOfWork(
        ApplicationDbContext context,
        ILogger<UnitOfWork> logger,
        IEmployeeRepository employeeRepository,
        IDependentRepository dependentRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _employees = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        _dependents = dependentRepository ?? throw new ArgumentNullException(nameof(dependentRepository));
    }

    /// <summary>
    /// Gets the Employee repository
    /// </summary>
    public IEmployeeRepository Employees => _employees;

    /// <summary>
    /// Gets the Dependent repository
    /// </summary>
    public IDependentRepository Dependents => _dependents;

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _logger.LogDebug("Beginning database transaction");

            // Check if the database provider supports transactions
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                _logger.LogDebug("InMemory database provider detected, skipping transaction creation");
                return;
            }

            _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error beginning database transaction");
            throw;
        }
    }

    /// <summary>
    /// Commits the current database transaction
    /// </summary>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                _logger.LogDebug("InMemory database provider detected, skipping transaction commit");
                return;
            }

            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            _logger.LogDebug("Committing database transaction");
            await _transaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing database transaction");
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Rolls back the current database transaction
    /// </summary>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
            {
                _logger.LogDebug("InMemory database provider detected, skipping transaction rollback");
                return;
            }

            if (_transaction == null)
            {
                _logger.LogWarning("Attempted to rollback transaction, but no transaction in progress");
                return;
            }

            _logger.LogDebug("Rolling back database transaction");
            await _transaction.RollbackAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back database transaction");
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Saving changes to database");
            var result = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("Successfully saved {ChangeCount} changes to database", result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to database");
            throw;
        }
    }

    /// <summary>
    /// Executes a function within a database transaction
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        if (operation == null)
            throw new ArgumentNullException(nameof(operation));

        var wasTransactionStarted = _transaction != null;

        try
        {
            if (!wasTransactionStarted)
            {
                await BeginTransactionAsync(cancellationToken);
            }

            var result = await operation();

            if (!wasTransactionStarted)
            {
                await CommitTransactionAsync(cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing operation in transaction");

            if (!wasTransactionStarted && _transaction != null)
            {
                await RollbackTransactionAsync(cancellationToken);
            }

            throw;
        }
    }

    /// <summary>
    /// Executes an action within a database transaction
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await operation();
            return true;
        }, cancellationToken);
    }

    /// <summary>
    /// Disposes the current transaction
    /// </summary>
    private async Task DisposeTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the Unit of Work and all its resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            try
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Unit of Work transaction");
            }

            _disposed = true;
        }
    }
}
