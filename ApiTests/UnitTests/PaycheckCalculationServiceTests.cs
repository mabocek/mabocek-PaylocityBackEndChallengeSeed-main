using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Api.Services;
using Api.Dtos.Employee;
using Api.Dtos.Dependent;
using Api.Dtos.Paycheck;
using Api.Features.Employees.Queries;
using Api.Models;
using Microsoft.FeatureManagement;
using Api.Features;
using MediatR;

namespace ApiTests.UnitTests;

/// <summary>
/// Unit tests for the refactored PaycheckCalculationService to verify business logic compliance.
/// Tests individual calculation methods in isolation for better testability.
/// </summary>
public class PaycheckCalculationServiceTests
{
    /// <summary>
    /// Creates a mock feature manager with all features enabled for testing
    /// </summary>
    private static Mock<IFeatureManager> CreateMockFeatureManagerAllEnabled()
    {
        var mockFeatureManager = new Mock<IFeatureManager>();
        mockFeatureManager.Setup(x => x.IsEnabledAsync(It.IsAny<string>()))
                         .ReturnsAsync(true);
        return mockFeatureManager;
    }

    /// <summary>
    /// Creates a mock feature manager with all features disabled for testing
    /// </summary>
    private static Mock<IFeatureManager> CreateMockFeatureManagerAllDisabled()
    {
        var mockFeatureManager = new Mock<IFeatureManager>();
        mockFeatureManager.Setup(x => x.IsEnabledAsync(It.IsAny<string>()))
                         .ReturnsAsync(false);
        return mockFeatureManager;
    }

    /// <summary>
    /// Creates a PaycheckCalculationService instance with all features enabled
    /// </summary>
    private static PaycheckCalculationService CreateServiceWithFeaturesEnabled()
    {
        var mockLogger = new Mock<ILogger<PaycheckCalculationService>>();
        return new PaycheckCalculationService(CreateMockFeatureManagerAllEnabled().Object, mockLogger.Object);
    }

    /// <summary>
    /// Creates a PaycheckCalculationService instance with all features disabled
    /// </summary>
    private static PaycheckCalculationService CreateServiceWithFeaturesDisabled()
    {
        var mockLogger = new Mock<ILogger<PaycheckCalculationService>>();
        return new PaycheckCalculationService(CreateMockFeatureManagerAllDisabled().Object, mockLogger.Object);
    }

    /// <summary>
    /// Tests gross pay calculation for different salary amounts
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(52000, 2000)] // $52,000 / 26 = $2,000
    [InlineData(75420.99, 2900.81)] // Test case from LeBron James
    [InlineData(92365.22, 3552.51)] // Test case from Ja Morant
    [InlineData(143211.12, 5508.12)] // Test case from Michael Jordan
    public void CalculateGrossPayPerPaycheck_VariousSalaries_ShouldCalculateCorrectly(
        decimal salary,
        decimal expectedGrossPay)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act
        var result = service.CalculateGrossPayPerPaycheck(salary);

        // Assert
        Assert.Equal(expectedGrossPay, result, 2);
    }

    /// <summary>
    /// Tests gross pay calculation with invalid input
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(-1000)] // Negative salary
    [InlineData(-0.01)] // Negative decimal
    [InlineData(-50000)] // Large negative value
    public void CalculateGrossPayPerPaycheck_InvalidSalary_ShouldThrowException(decimal invalidSalary)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.CalculateGrossPayPerPaycheck(invalidSalary));
    }

    /// <summary>
    /// Tests age calculation for different dates of birth
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData("1990-01-01", 35)] // Born Jan 1, 1990, assuming current date is after Jan 1, 2025
    [InlineData("1974-01-02", 51)] // Test case from Michael Jordan's dependent
    [InlineData("2020-06-23", 4)]  // Test case from Ja Morant's child
    public void CalculateAge_VariousDatesOfBirth_ShouldCalculateCorrectly(
        string dateOfBirthString,
        int expectedAge)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();
        var dateOfBirth = DateOnly.Parse(dateOfBirthString);

        // Act
        var result = service.CalculateAge(dateOfBirth);

        // Assert
        Assert.Equal(expectedAge, (int)result);
    }

    /// <summary>
    /// Tests per-paycheck deduction calculation
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(1000, 461.54)] // $1,000 monthly = $12,000 yearly / 26 paychecks = $461.54
    [InlineData(1600, 738.46)] // $1,600 monthly = $19,200 yearly / 26 paychecks = $738.46
    [InlineData(2953.94, 1363.36)] // Test case from Ja Morant
    public void CalculatePerPaycheckDeduction_VariousMonthlyCosts_ShouldCalculateCorrectly(
        decimal monthlyCost,
        decimal expectedPerPaycheckDeduction)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act
        var result = service.CalculatePerPaycheckDeduction(monthlyCost);

        // Assert
        Assert.Equal(expectedPerPaycheckDeduction, result, 2);
    }

    /// <summary>
    /// Tests per-paycheck deduction calculation with invalid input
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(-100)] // Negative cost
    [InlineData(-0.01)] // Small negative decimal
    [InlineData(-1000)] // Large negative value
    public void CalculatePerPaycheckDeduction_InvalidCost_ShouldThrowException(decimal invalidCost)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => service.CalculatePerPaycheckDeduction(invalidCost));
    }

    /// <summary>
    /// Tests high salary additional cost calculation with different salaries
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(50000, 0)] // Under threshold
    [InlineData(80000, 0)] // At threshold
    [InlineData(80001, 133.34)] // Just over threshold: $80,001 * 2% / 12 = $133.34
    [InlineData(92365.22, 153.94)] // Test case from Ja Morant: $92,365.22 * 2% / 12 = $153.94
    [InlineData(143211.12, 238.69)] // Test case from Michael Jordan: $143,211.12 * 2% / 12 = $238.69
    public async Task CalculateHighSalaryAdditionalCostAsync_VariousSalaries_ShouldCalculateCorrectly(
        decimal salary,
        decimal expectedAdditionalCost)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act
        var result = await service.CalculateHighSalaryAdditionalCostAsync(salary);

        // Assert
        Assert.Equal(expectedAdditionalCost, result, 2);
    }

    /// <summary>
    /// Tests high salary additional cost calculation when feature is disabled
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(92365.22)]
    [InlineData(143211.12)]
    public async Task CalculateHighSalaryAdditionalCostAsync_FeatureDisabled_ShouldReturnZero(decimal salary)
    {
        // Arrange
        var service = CreateServiceWithFeaturesDisabled();

        // Act
        var result = await service.CalculateHighSalaryAdditionalCostAsync(salary);

        // Assert
        Assert.Equal(0m, result);
    }

    /// <summary>
    /// Tests dependent cost calculation for different ages
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(25, 600, 0)] // Under 50
    [InlineData(50, 600, 200)] // Exactly 50
    [InlineData(51, 600, 200)] // Over 50
    public async Task CalculateDependentCostAsync_VariousAges_ShouldCalculateCorrectly(
        int age,
        decimal expectedBaseCost,
        decimal expectedSeniorCost)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();
        var birthYear = DateTime.Today.Year - age;
        var dependent = new GetDependentDto(
            1,
            "Test",
            "Dependent",
            new DateOnly(birthYear, 1, 1),
            Relationship.Spouse,
            1);

        // Act
        var result = await service.CalculateDependentCostAsync(dependent);

        // Assert
        Assert.Equal(expectedBaseCost, result.BaseCost);
        Assert.Equal(expectedSeniorCost, result.SeniorAdditionalCost);
        Assert.Equal(expectedBaseCost + expectedSeniorCost, result.TotalCost);
    }

    /// <summary>
    /// Tests dependent cost calculation when over-50 feature is disabled
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData("1970-01-01", "Senior", "Spouse")] // Over 50 spouse
    [InlineData("1965-06-15", "Elder", "Child")] // Over 50 child
    [InlineData("1960-12-31", "Older", "DomesticPartner")] // Over 50 domestic partner
    public async Task CalculateDependentCostAsync_SeniorFeatureDisabled_ShouldNotAddSurcharge(
        string dateOfBirthString,
        string lastName,
        string relationshipString)
    {
        // Arrange
        var service = CreateServiceWithFeaturesDisabled();
        var relationship = Enum.Parse<Relationship>(relationshipString);
        var dependent = new GetDependentDto(
            1,
            "Test",
            lastName,
            DateOnly.Parse(dateOfBirthString),
            relationship,
            1);

        // Act
        var result = await service.CalculateDependentCostAsync(dependent);

        // Assert
        Assert.Equal(600m, result.BaseCost);
        Assert.Equal(0m, result.SeniorAdditionalCost);
        Assert.Equal(600m, result.TotalCost);
    }

    /// <summary>
    /// Tests dependent cost calculation with null input
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    public async Task CalculateDependentCostAsync_NullDependent_ShouldThrowException()
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CalculateDependentCostAsync(null!));
    }

    /// <summary>
    /// Tests full paycheck calculation for employees without dependents
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [InlineData(1, "John", "Doe", 52000, "1990-01-01", 2000, 461.54, 1538.46)] // Standard case
    [InlineData(2, "Jane", "Smith", 75000, "1985-03-15", 2884.62, 461.54, 2423.08)] // Higher salary
    [InlineData(3, "Bob", "Johnson", 35000, "1992-07-22", 1346.15, 461.54, 884.62)] // Lower salary - corrected rounding
    public async Task CalculatePaycheckAsync_EmployeeWithoutDependents_ShouldCalculateCorrectly(
        int employeeId,
        string firstName,
        string lastName,
        decimal salary,
        string dateOfBirthString,
        decimal expectedGrossPay,
        decimal expectedBenefitDeductions,
        decimal expectedNetPay)
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();
        var employee = new GetEmployeeDto(
            employeeId,
            firstName,
            lastName,
            salary,
            DateOnly.Parse(dateOfBirthString),
            new List<GetDependentDto>());

        // Act
        var result = await service.CalculatePaycheckAsync(employee);

        // Assert
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal($"{firstName} {lastName}", result.EmployeeName);
        Assert.Equal(expectedGrossPay, result.GrossPay, 2);
        Assert.Equal(expectedBenefitDeductions, result.BenefitDeductions, 2);
        Assert.Equal(expectedNetPay, result.NetPay, 2);
    }

    /// <summary>
    /// Tests full paycheck calculation with null employee
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    public async Task CalculatePaycheckAsync_NullEmployee_ShouldThrowException()
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CalculatePaycheckAsync(null!));
    }

    /// <summary>
    /// Tests benefit details calculation with null employee
    /// </summary>
    [Fact]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    public async Task CalculateBenefitDetailsAsync_NullEmployee_ShouldThrowException()
    {
        // Arrange
        var service = CreateServiceWithFeaturesEnabled();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CalculateBenefitDetailsAsync(null!));
    }
}
