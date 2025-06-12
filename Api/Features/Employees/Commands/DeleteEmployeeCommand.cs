using MediatR;
using Api.Repositories;

namespace Api.Features.Employees.Commands;

/// <summary>
/// Command to delete an employee
/// </summary>
public record DeleteEmployeeCommand(int Id) : IRequest<bool>;

/// <summary>
/// Handler for deleting an employee
/// </summary>
public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteEmployeeCommandHandler> _logger;

    public DeleteEmployeeCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing DeleteEmployeeCommand for employee {EmployeeId}", request.Id);

            // Execute within transaction for data consistency
            var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // Check if employee exists using repository through Unit of Work
                var exists = await _unitOfWork.Employees.ExistsAsync(request.Id, cancellationToken);
                if (!exists)
                {
                    _logger.LogWarning("Employee with id {EmployeeId} not found for deletion", request.Id);
                    return false;
                }

                // Delete employee using repository through Unit of Work (cascade delete will handle dependents)
                await _unitOfWork.Employees.DeleteByIdAsync(request.Id, cancellationToken);

                // Save changes within transaction
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return true;
            }, cancellationToken);

            _logger.LogInformation("Successfully deleted employee {EmployeeId}", request.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DeleteEmployeeCommand for employee {EmployeeId}", request.Id);
            throw;
        }
    }
}
