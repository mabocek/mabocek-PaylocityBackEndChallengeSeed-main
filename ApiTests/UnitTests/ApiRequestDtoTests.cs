using System;
using Api.Endpoints;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Unit tests for API request DTOs to ensure they work correctly
/// These are simple record types but testing them provides coverage for the 0% items
/// </summary>
public class ApiRequestDtoTests
{
    #region CreateEmployeeRequest Tests

    [Fact]
    public void CreateEmployeeRequest_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var request = new CreateEmployeeRequest(
            "John",
            "Doe",
            50000m,
            new DateOnly(1990, 1, 1));

        // Assert
        Assert.Equal("John", request.FirstName);
        Assert.Equal("Doe", request.LastName);
        Assert.Equal(50000m, request.Salary);
        Assert.Equal(new DateOnly(1990, 1, 1), request.DateOfBirth);
    }

    [Fact]
    public void CreateEmployeeRequest_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var request1 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var request2 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var request3 = new CreateEmployeeRequest("Jane", "Doe", 50000m, new DateOnly(1990, 1, 1));

        // Assert
        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }

    [Fact]
    public void CreateEmployeeRequest_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var request = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));

        // Act
        var result = request.ToString();

        // Assert
        Assert.Contains("John", result);
        Assert.Contains("Doe", result);
        Assert.Contains("50000", result);
        Assert.Contains("1990", result);
    }

    [Fact]
    public void CreateEmployeeRequest_GetHashCode_IsConsistent()
    {
        // Arrange
        var request1 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));
        var request2 = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));

        // Act & Assert
        Assert.Equal(request1.GetHashCode(), request2.GetHashCode());
    }

    [Fact]
    public void CreateEmployeeRequest_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var request = new CreateEmployeeRequest("John", "Doe", 50000m, new DateOnly(1990, 1, 1));

        // Act
        var (firstName, lastName, salary, dateOfBirth) = request;

        // Assert
        Assert.Equal("John", firstName);
        Assert.Equal("Doe", lastName);
        Assert.Equal(50000m, salary);
        Assert.Equal(new DateOnly(1990, 1, 1), dateOfBirth);
    }

    #endregion

    #region UpdateEmployeeRequest Tests

    [Fact]
    public void UpdateEmployeeRequest_CanBeCreated_WithValidData()
    {
        // Arrange & Act
        var request = new UpdateEmployeeRequest(
            "Jane",
            "Smith",
            75000m,
            new DateOnly(1985, 5, 15));

        // Assert
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Smith", request.LastName);
        Assert.Equal(75000m, request.Salary);
        Assert.Equal(new DateOnly(1985, 5, 15), request.DateOfBirth);
    }

    [Fact]
    public void UpdateEmployeeRequest_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var request1 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));
        var request2 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));
        var request3 = new UpdateEmployeeRequest("John", "Smith", 75000m, new DateOnly(1985, 5, 15));

        // Assert
        Assert.Equal(request1, request2);
        Assert.NotEqual(request1, request3);
    }

    [Fact]
    public void UpdateEmployeeRequest_ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var request = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));

        // Act
        var result = request.ToString();

        // Assert
        Assert.Contains("Jane", result);
        Assert.Contains("Smith", result);
        Assert.Contains("75000", result);
        Assert.Contains("1985", result);
    }

    [Fact]
    public void UpdateEmployeeRequest_GetHashCode_IsConsistent()
    {
        // Arrange
        var request1 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));
        var request2 = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));

        // Act & Assert
        Assert.Equal(request1.GetHashCode(), request2.GetHashCode());
    }

    [Fact]
    public void UpdateEmployeeRequest_Deconstruction_WorksCorrectly()
    {
        // Arrange
        var request = new UpdateEmployeeRequest("Jane", "Smith", 75000m, new DateOnly(1985, 5, 15));

        // Act
        var (firstName, lastName, salary, dateOfBirth) = request;

        // Assert
        Assert.Equal("Jane", firstName);
        Assert.Equal("Smith", lastName);
        Assert.Equal(75000m, salary);
        Assert.Equal(new DateOnly(1985, 5, 15), dateOfBirth);
    }

    #endregion

    #region Edge Cases and Boundary Values

    [Fact]
    public void CreateEmployeeRequest_WithEmptyStrings_StoresCorrectly()
    {
        // Arrange & Act
        var request = new CreateEmployeeRequest("", "", 0m, new DateOnly(1, 1, 1));

        // Assert
        Assert.Equal("", request.FirstName);
        Assert.Equal("", request.LastName);
        Assert.Equal(0m, request.Salary);
        Assert.Equal(new DateOnly(1, 1, 1), request.DateOfBirth);
    }

    [Fact]
    public void UpdateEmployeeRequest_WithEmptyStrings_StoresCorrectly()
    {
        // Arrange & Act
        var request = new UpdateEmployeeRequest("", "", 0m, new DateOnly(1, 1, 1));

        // Assert
        Assert.Equal("", request.FirstName);
        Assert.Equal("", request.LastName);
        Assert.Equal(0m, request.Salary);
        Assert.Equal(new DateOnly(1, 1, 1), request.DateOfBirth);
    }

    [Fact]
    public void CreateEmployeeRequest_WithMaxValues_HandledCorrectly()
    {
        // Arrange & Act
        var request = new CreateEmployeeRequest(
            new string('A', 100),
            new string('B', 100),
            decimal.MaxValue,
            DateOnly.MaxValue);

        // Assert
        Assert.Equal(new string('A', 100), request.FirstName);
        Assert.Equal(new string('B', 100), request.LastName);
        Assert.Equal(decimal.MaxValue, request.Salary);
        Assert.Equal(DateOnly.MaxValue, request.DateOfBirth);
    }

    [Fact]
    public void UpdateEmployeeRequest_WithMaxValues_HandledCorrectly()
    {
        // Arrange & Act
        var request = new UpdateEmployeeRequest(
            new string('A', 100),
            new string('B', 100),
            decimal.MaxValue,
            DateOnly.MaxValue);

        // Assert
        Assert.Equal(new string('A', 100), request.FirstName);
        Assert.Equal(new string('B', 100), request.LastName);
        Assert.Equal(decimal.MaxValue, request.Salary);
        Assert.Equal(DateOnly.MaxValue, request.DateOfBirth);
    }

    #endregion
}
