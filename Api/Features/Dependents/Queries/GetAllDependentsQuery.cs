using MediatR;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Dependents.Queries;

/// <summary>
/// Query to get all dependents
/// </summary>
public record GetAllDependentsQuery : IRequest<List<GetDependentDto>>;

/// <summary>
/// Handler for getting all dependents
/// </summary>
public class GetAllDependentsQueryHandler : IRequestHandler<GetAllDependentsQuery, List<GetDependentDto>>
{
    private readonly IDependentRepository _dependentRepository;
    private readonly ILogger<GetAllDependentsQueryHandler> _logger;

    public GetAllDependentsQueryHandler(IDependentRepository dependentRepository, ILogger<GetAllDependentsQueryHandler> logger)
    {
        _dependentRepository = dependentRepository ?? throw new ArgumentNullException(nameof(dependentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<GetDependentDto>> Handle(GetAllDependentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetAllDependentsQuery");

            var dependents = await _dependentRepository.GetAllAsync(cancellationToken);

            var result = dependents.Select(MapToDto).ToList();

            _logger.LogInformation("Successfully retrieved {Count} dependents", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetAllDependentsQuery");
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
