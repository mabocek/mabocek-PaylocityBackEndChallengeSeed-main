using Api.Dtos.Dependent;

namespace Api.Dtos.Employee;

public record GetEmployeeDto(
    int Id,
    string FirstName,
    string LastName,
    decimal Salary,
    // Based on requirements, time is not critical for this application,
    // we can use DateOnly to represent just the date part.
    //
    // If this would be critical date for calculations,
    // we could use DateTimeOffset, to make sure we account for time zone differences.
    DateOnly DateOfBirth,
    // Improved encapsulation.
    IReadOnlyCollection<GetDependentDto> Dependents
)
{
    public IReadOnlyCollection<GetDependentDto> Dependents { get; init; } = Dependents ?? new List<GetDependentDto>();
}
