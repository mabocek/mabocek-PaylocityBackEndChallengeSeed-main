using System;
using System.Collections.Generic;
using Api.Features.Employees.Commands;
using Api.Models;
using Api.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Simplified tests for CreateEmployeeWithDependentsCommand focusing on essential validation
/// Note: Complex business logic validation (relationship constraints) is tested through 
/// integration tests which provide better coverage with proper database contexts.
/// </summary>
public class CreateEmployeeWithDependentsCommandTests
{
    [Theory]
    [InlineData("John", "Doe", 50000, 1990, 1, 1)]
    [InlineData("", "", 0, 1, 1, 1)]
    [InlineData("VeryLongFirstName", "VeryLongLastName", 999999.99, 2000, 12, 31)]
    public void CreateEmployeeWithDependentsCommand_StoresDataCorrectly(
        string firstName, string lastName, decimal salary, int year, int month, int day)
    {
        // Arrange
        var dependents = new List<CreateDependentDto>
        {
            new("Child1", "LastName", new DateOnly(2010, 1, 1), Relationship.Child)
        };

        // Act
        var command = new CreateEmployeeWithDependentsCommand(
            firstName, lastName, salary, new DateOnly(year, month, day), dependents);

        // Assert
        Assert.Equal(firstName, command.FirstName);
        Assert.Equal(lastName, command.LastName);
        Assert.Equal(salary, command.Salary);
        Assert.Equal(new DateOnly(year, month, day), command.DateOfBirth);
        Assert.Equal(dependents, command.Dependents);
    }

    [Theory]
    [InlineData("Jane", "Smith", 1985, 5, 15, Relationship.Spouse)]
    [InlineData("John", "Doe", 2010, 12, 25, Relationship.Child)]
    [InlineData("Partner", "Name", 1990, 6, 30, Relationship.DomesticPartner)]
    public void CreateDependentDto_StoresDataCorrectly(
        string firstName, string lastName, int year, int month, int day, Relationship relationship)
    {
        // Arrange & Act
        var dto = new CreateDependentDto(firstName, lastName, new DateOnly(year, month, day), relationship);

        // Assert
        Assert.Equal(firstName, dto.FirstName);
        Assert.Equal(lastName, dto.LastName);
        Assert.Equal(new DateOnly(year, month, day), dto.DateOfBirth);
        Assert.Equal(relationship, dto.Relationship);
    }

    [Fact]
    public void CreateEmployeeWithDependentsCommand_RecordEquality_WorksCorrectly()
    {
        var dependents = new List<CreateDependentDto>
        {
            new("Child", "Name", new DateOnly(2010, 1, 1), Relationship.Child)
        };

        var command1 = new CreateEmployeeWithDependentsCommand("John", "Doe", 50000m, new DateOnly(1990, 1, 1), dependents);
        var command2 = new CreateEmployeeWithDependentsCommand("John", "Doe", 50000m, new DateOnly(1990, 1, 1), dependents);
        var command3 = new CreateEmployeeWithDependentsCommand("Jane", "Doe", 50000m, new DateOnly(1990, 1, 1), dependents);

        Assert.Equal(command1, command2);
        Assert.NotEqual(command1, command3);
    }

    [Fact]
    public void CreateDependentDto_RecordEquality_WorksCorrectly()
    {
        var dto1 = new CreateDependentDto("Jane", "Smith", new DateOnly(1985, 5, 15), Relationship.Spouse);
        var dto2 = new CreateDependentDto("Jane", "Smith", new DateOnly(1985, 5, 15), Relationship.Spouse);
        var dto3 = new CreateDependentDto("John", "Smith", new DateOnly(1985, 5, 15), Relationship.Spouse);

        Assert.Equal(dto1, dto2);
        Assert.NotEqual(dto1, dto3);
    }

    [Fact]
    public void CreateEmployeeWithDependentsCommandHandler_NullDependencies_ThrowsArgumentNullException()
    {
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<CreateEmployeeWithDependentsCommandHandler>>();

        Assert.Throws<ArgumentNullException>(() => new CreateEmployeeWithDependentsCommandHandler(null!, mockLogger.Object));
        Assert.Throws<ArgumentNullException>(() => new CreateEmployeeWithDependentsCommandHandler(mockUnitOfWork.Object, null!));
    }
}
