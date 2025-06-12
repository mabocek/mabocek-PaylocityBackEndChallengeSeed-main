namespace Api.Repositories;

/// <summary>
/// Unit of Work interface for managing transactions and repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the Employee repository
    /// </summary>
    IEmployeeRepository Employees { get; }

    /// <summary>
    /// Gets the Dependent repository
    /// </summary>
    IDependentRepository Dependents { get; }

    /// <summary>
    /// Gets a generic repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    /// <returns>Repository instance</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commits the current database transaction
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current database transaction
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves all changes to the database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a function within a database transaction
    /// </summary>
    /// <typeparam name="TResult">Return type</typeparam>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Operation result</returns>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action within a database transaction
    /// </summary>
    /// <param name="operation">Operation to execute</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}
