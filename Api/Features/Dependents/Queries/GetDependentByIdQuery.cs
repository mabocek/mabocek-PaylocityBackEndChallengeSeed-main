using MediatR;
using Api.Dtos.Dependent;
using Api.Models;
using Api.Repositories;

namespace Api.Features.Dependents.Queries;

/// <summary>
/// Query to get dependent by id
/// </summary>
public record GetDependentByIdQuery(int Id) : IRequest<GetDependentDto?>;

/// <summary>
/// Handler for getting dependent by id
/// </summary>
public class GetDependentByIdQueryHandler : IRequestHandler<GetDependentByIdQuery, GetDependentDto?>
{
    private readonly IDependentRepository _dependentRepository;
    private readonly ILogger<GetDependentByIdQueryHandler> _logger;

    public GetDependentByIdQueryHandler(IDependentRepository dependentRepository, ILogger<GetDependentByIdQueryHandler> logger)
    {
        _dependentRepository = dependentRepository ?? throw new ArgumentNullException(nameof(dependentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetDependentDto?> Handle(GetDependentByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing GetDependentByIdQuery for dependent {DependentId}", request.Id);

            var dependent = await _dependentRepository.GetByIdAsync(request.Id, cancellationToken);

            if (dependent == null)
            {
                _logger.LogWarning("Dependent with id {DependentId} not found", request.Id);
                return null;
            }

            var result = MapToDto(dependent);
            _logger.LogInformation("Successfully retrieved dependent {DependentId}", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing GetDependentByIdQuery for dependent {DependentId}", request.Id);
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
