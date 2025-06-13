using Api.Dtos.Dependent;
using Api.Models;
using Api.Features.Dependents.Queries;
using Api.Features;
using MediatR;
using Microsoft.FeatureManagement;

namespace Api.Services;

public class DependentService : BaseService, IDependentService
{
    private const string SortByFirstName = "firstname";
    private const string SortByLastName = "lastname";
    private const string SortByDateOfBirth = "dateofbirth";
    private const string SortByRelationship = "relationship";
    private const string SortByEmployeeId = "employeeid";

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

    public async Task<ApiResponse<GetDependentDto>> GetDependentByIdAsync(int id)
    {
        // Check feature flag
        if (!await _featureManager.IsEnabledAsync(FeatureFlags.EnableDependentOperations))
        {
            return Failure<GetDependentDto>("Dependent operations feature is currently disabled");
        }

        return await ExecuteAsync(async () =>
        {
            var dependent = await GetByIdAsync(id);
            if (dependent == null)
            {
                throw new ArgumentException($"Dependent with id {id} not found");
            }
            return dependent;
        }, "retrieving dependent");
    }

    public async Task<ApiResponse<PagedResult<GetDependentDto>>> GetDependentsPagedAsync(
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
            return Failure<PagedResult<GetDependentDto>>("Dependent operations feature is currently disabled");
        }

        return await ExecuteAsync(async () =>
        {
            // Convert parameters
            var relationshipEnum = ParseEnum<Relationship>(relationship);
            var ascending = ParseSortOrder(sortOrder);

            return await GetPagedAsync(page, pageSize, employeeId, relationshipEnum, sortBy, ascending);
        }, "retrieving dependents");
    }
}