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
}
