namespace Api.Features;

/// <summary>
/// Constants for all feature flags used in the application
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Controls whether paycheck calculation functionality is enabled
    /// </summary>
    public const string EnablePaycheckCalculation = nameof(EnablePaycheckCalculation);

    /// <summary>
    /// Controls whether dependent CRUD operations are enabled
    /// </summary>
    public const string EnableDependentOperations = nameof(EnableDependentOperations);

    /// <summary>
    /// Controls whether high salary calculation (>$80K) is applied
    /// </summary>
    public const string EnableHighSalaryCalculation = nameof(EnableHighSalaryCalculation);

    /// <summary>
    /// Controls whether senior dependent surcharge is applied
    /// </summary>
    public const string EnableSeniorDependentSurcharge = nameof(EnableSeniorDependentSurcharge);

    /// <summary>
    /// Controls whether detailed paycheck breakdown is included in responses
    /// </summary>
    public const string EnableDetailedPaycheckBreakdown = nameof(EnableDetailedPaycheckBreakdown);

    /// <summary>
    /// Controls whether Swagger UI is enabled
    /// </summary>
    public const string EnableSwaggerUI = nameof(EnableSwaggerUI);

    /// <summary>
    /// Controls whether bulk operations are enabled
    /// </summary>
    public const string EnableBulkOperations = nameof(EnableBulkOperations);

    /// <summary>
    /// Controls whether advanced logging is enabled
    /// </summary>
    public const string EnableAdvancedLogging = nameof(EnableAdvancedLogging);

    /// <summary>
    /// Controls whether response caching is enabled
    /// </summary>
    public const string EnableCaching = nameof(EnableCaching);

    /// <summary>
    /// Controls whether rate limiting is enabled
    /// </summary>
    public const string EnableRateLimiting = nameof(EnableRateLimiting);
}
