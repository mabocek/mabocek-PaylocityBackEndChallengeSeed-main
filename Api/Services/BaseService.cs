using Api.Models;
using Microsoft.FeatureManagement;

namespace Api.Services;

/// <summary>
/// Base service class providing common functionality for all services
/// </summary>
public abstract class BaseService
{
    protected readonly IFeatureManager _featureManager;
    protected readonly ILogger _logger;

    protected BaseService(IFeatureManager featureManager, ILogger logger)
    {
        _featureManager = featureManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates a successful API response
    /// </summary>
    protected ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = true,
            Message = message ?? string.Empty
        };
    }

    /// <summary>
    /// Creates a failed API response
    /// </summary>
    protected ApiResponse<T> Failure<T>(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = false,
            Message = message
        };
    }

    /// <summary>
    /// Converts string sort order to boolean (ascending)
    /// </summary>
    protected bool ParseSortOrder(string? sortOrder) =>
        sortOrder?.ToLower() != "desc";

    /// <summary>
    /// Safely converts string to enum
    /// </summary>
    protected T? ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return Enum.TryParse<T>(value, true, out var result) ? result : null;
    }

    /// <summary>
    /// Executes an operation with exception handling and logging
    /// </summary>
    protected async Task<ApiResponse<T>> ExecuteAsync<T>(Func<Task<T>> operation, string operationName)
    {
        try
        {
            var result = await operation();
            return Success(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in {OperationName}", operationName);
            return Failure<T>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {OperationName}", operationName);
            return Failure<T>($"An error occurred while {operationName.ToLower()}");
        }
    }

    /// <summary>
    /// Executes an operation with exception handling, logging, and custom success message
    /// </summary>
    protected async Task<ApiResponse<T>> ExecuteAsync<T>(Func<Task<T>> operation, string operationName, string? successMessage)
    {
        try
        {
            var result = await operation();
            return Success(result, successMessage);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error in {OperationName}", operationName);
            return Failure<T>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in {OperationName}", operationName);
            return Failure<T>($"An error occurred while {operationName.ToLower()}");
        }
    }
}
