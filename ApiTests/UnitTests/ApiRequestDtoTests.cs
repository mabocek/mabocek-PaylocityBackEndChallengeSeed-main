using System;
using Api.Endpoints;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Simplified tests for API request DTOs - focuses on essential functionality only
/// </summary>
public class ApiRequestDtoTests
{
    [Theory]
    [InlineData("John", "Doe", 50000, 1990, 1, 1)]
    [InlineData("", "", 0, 1, 1, 1)]
    [InlineData("VeryLongFirstName", "VeryLongLastName", 999999.99, 2000, 12, 31)]
    public void CreateEmployeeRequest_StoresDataCorrectly(
        string firstName, string lastName, decimal salary, int year, int month, int day)
    {
        // Arrange & Act
        var request = new CreateEmployeeRequest(firstName, lastName, salary, new DateOnly(year, month, day));

        // Assert
        Assert.Equal(firstName, request.FirstName);
        Assert.Equal(lastName, request.LastName);
        Assert.Equal(salary, request.Salary);
        Assert.Equal(new DateOnly(year, month, day), request.DateOfBirth);
    }

    [Theory]
    [InlineData("Jane", "Smith", 75000, 1985, 5, 15)]
    [InlineData("", "", 0, 1, 1, 1)]
    [InlineData("VeryLongFirstName", "VeryLongLastName", 999999.99, 2000, 12, 31)]
    public void UpdateEmployeeRequest_StoresDataCorrectly(
        string firstName, string lastName, decimal salary, int year, int month, int day)
    {
        // Arrange & Act
        var request = new UpdateEmployeeRequest(firstName, lastName, salary, new DateOnly(year, month, day));

        // Assert
        Assert.Equal(firstName, request.FirstName);
        Assert.Equal(lastName, request.LastName);
        Assert.Equal(salary, request.Salary);
        Assert.Equal(new DateOnly(year, month, day), request.DateOfBirth);
    }

    [Fact]
    public void CreateEmployeeRequest_RecordEquality_WorksCorrectly()
    {
        var request1 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var request2 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var request3 = new CreateEmployeeRequest("Jane", "Doe", 50000m, new DateOnly(1990, 1, 1));

        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }

    [Fact]
    public void UpdateEmployeeRequest_RecordEquality_WorksCorrectly()
    {
        var request1 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));
        var request2 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));
        var request3 = new UpdateEmployeeRequest("John", "Smith", 75000m, new DateOnly(1985, 5, 15));

        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }
}