using MediatR;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Dependents.Queries;

/// <summary>
/// Query to get paginated dependents
/// </summary>
public record GetPagedDependentsQuery(
    int Page,
    int PageSize,
    int? EmployeeId = null,
    Relationship? Relationship = null,
    string? SortBy = null,
    bool Ascending = true) : IRequest<PagedResult<GetDependentDto>>;

/// <summary>
/// Handler for getting paginated dependents
/// </summary>
public class GetPagedDependentsQueryHandler : IRequestHandler<GetPagedDependentsQuery, PagedResult<GetDependentDto>>
{
    private readonly IDependentRepository _dependentRepository;
    private readonly ILogger<GetPagedDependentsQueryHandler> _logger;

    public GetPagedDependentsQueryHandler(IDependentRepository dependentRepository, ILogger<GetPagedDependentsQueryHandler> logger)
    {
        _dependentRepository = dependentRepository ?? throw new ArgumentNullException(nameof(dependentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<GetDependentDto>> Handle(GetPagedDependentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetPagedDependentsQuery - Page: {Page}, PageSize: {PageSize}, EmployeeId: {EmployeeId}, Relationship: {Relationship}, SortBy: {SortBy}, Ascending: {Ascending}",
                request.Page, request.PageSize, request.EmployeeId, request.Relationship, request.SortBy, request.Ascending);

            var pagedDependents = await _dependentRepository.GetPagedAsync(
                request.Page,
                request.PageSize,
                request.EmployeeId,
                request.Relationship,
                request.SortBy,
                request.Ascending,
                cancellationToken);

            var result = new PagedResult<GetDependentDto>(
                pagedDependents.Items.Select(MapToDto).ToList(),
                pagedDependents.TotalItems,
                pagedDependents.CurrentPage,
                pagedDependents.PageSize);

            _logger.LogInformation("Successfully retrieved {Count} dependents out of {TotalItems} total",
                result.Items.Count, result.TotalItems);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetPagedDependentsQuery");
            throw;
        }
    }

    private static GetDependentDto MapToDto(Dependent dependent)
    {
        return new GetDependentDto(
            dependent.Id,
            dependent.FirstName,
            dependent.LastName,
            dependent.DateOfBirth,
            dependent.Relationship,
            dependent.EmployeeId
        );
    }
}
