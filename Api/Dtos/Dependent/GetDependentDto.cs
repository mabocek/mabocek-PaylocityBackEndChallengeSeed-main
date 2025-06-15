using Api.Models;

namespace Api.Dtos.Dependent;

public record GetDependentDto(
    int Id,
    string FirstName,
    string LastName,
    // Based on requirements, time is not critical for this application,
    // we can use DateOnly to represent just the date part.
    //
    // If this would be critical date for calculations,
    // we could use DateTimeOffset, to make sure we account for time zone differences.
    DateOnly DateOfBirth,
    Relationship Relationship,
    int EmployeeId
);
