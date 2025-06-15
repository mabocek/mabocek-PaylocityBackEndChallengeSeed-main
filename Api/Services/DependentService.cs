using Api.Dtos.Dependent;
using Api.Models;
using Api.Features.Dependents.Queries;
using Api.Features;
using Api.Constants;
using MediatR;
using Microsoft.FeatureManagement;

namespace Api.Services;

public class DependentService : BaseService, IDependentService
{
    private readonly IMediator _mediator;

    public DependentService(IMediator mediator, IFeatureManager featureManager, ILogger<DependentService> logger)
        : base(featureManager, logger)
    {
        _mediator = mediator;
    }

    public async Task<PagedResult<GetDependentDto>> GetPagedAsync(
        int page,
        int pageSize,
        int? employeeId = null,
        Relationship? relationship = null,
        string? sortBy = null,
        bool ascending = true)
    {
        return await _mediator.Send(new GetPagedDependentsQuery(
            page,
            pageSize,
            employeeId,
            relationship,
            sortBy,
            ascending));
    }

    public async Task<GetDependentDto?> GetByIdAsync(int id)
    {
        return await _mediator.Send(new GetDependentByIdQuery(id));
    }

    public async Task<GetDependentDto?> GetDependentByIdAsync(int id)
    {
        // Check feature flag
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
        {
            throw new InvalidOperationException("Dependent operations feature is currently disabled");
        }

        try
        {
            var dependent = await GetByIdAsync(id);
            return dependent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dependent {Id}", id);
            return null;
        }
    }

    public async Task<PagedResult<GetDependentDto>> GetDependentsPagedAsync(
        int page = 1,
        int pageSize = 10,
        int? employeeId = null,
        string? relationship = null,
        string? sortBy = null,
        string? sortOrder = null)
    {
        // Check feature flag
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
        {
            throw new InvalidOperationException("Dependent operations feature is currently disabled");
        }

        try
        {
            // Convert parameters
            var relationshipEnum = ParseEnum<Relationship>(relationship);
            var ascending = ParseSortOrder(sortOrder);

            return await GetPagedAsync(page, pageSize, employeeId, relationshipEnum, sortBy, ascending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged dependents");
            throw;
        }
    }
}