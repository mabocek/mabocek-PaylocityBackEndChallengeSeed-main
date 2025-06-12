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
using Api.Features.Employees.Queries;
using Api.Models;
using Microsoft.FeatureManagement;
using Api.Features;
using MediatR;

namespace ApiTests.UnitTests;

/// <summary>
/// Integration tests for paycheck calculations through EmployeeService.
/// Tests the full service integration with the refactored PaycheckCalculationService.
/// </summary>
public class PaycheckCalculationTests
{
    /// <summary>
    /// Creates a mock feature manager with all features enabled for testing
    /// </summary>
    private static Mock<IFeatureManager> CreateMockFeatureManager()
    {
        var mockFeatureManager = new Mock<IFeatureManager>();

        // Enable all features by default for testing
        mockFeatureManager.Setup(x => x.IsEnabledAsync(It.IsAny<string>()))
                         .ReturnsAsync(true);

        return mockFeatureManager;
    }

    /// <summary>
    /// Creates a real paycheck calculation service for integration testing
    /// </summary>
    private static IPaycheckCalculationService CreatePaycheckCalculationService()
    {
        var mockFeatureManager = CreateMockFeatureManager();
        var mockLogger = new Mock<ILogger<PaycheckCalculationService>>();

        return new PaycheckCalculationService(mockFeatureManager.Object, mockLogger.Object);
    }

    /// <summary>
    /// Maps Employee entity to EmployeeDto
    /// </summary>
    private static GetEmployeeDto MapToEmployeeDto(Employee employee)
    {
        return new GetEmployeeDto(
            employee.Id,
            employee.FirstName,
            employee.LastName,
            employee.Salary,
            employee.DateOfBirth,
            employee.Dependents.Select(d => new GetDependentDto(
                d.Id,
                d.FirstName,
                d.LastName,
                d.DateOfBirth,
                d.Relationship,
                d.EmployeeId
            )).ToList()
        );
    }

    /// <summary>
    /// Creates test employee data matching the original seeded data
    /// </summary>
    private static Employee CreateTestEmployee(int id, string firstName, string lastName, decimal salary, DateOnly dateOfBirth, List<Dependent>? dependents = null)
    {
        return new Employee
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Salary = salary,
            DateOfBirth = dateOfBirth,
            Dependents = dependents ?? new List<Dependent>()
        };
    }

    /// <summary>
    /// Creates test dependent data
    /// </summary>
    private static Dependent CreateTestDependent(int id, string firstName, string lastName, DateOnly dateOfBirth, Relationship relationship, int employeeId)
    {
        return new Dependent
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Relationship = relationship,
            EmployeeId = employeeId
        };
    }

    /// <summary>
    /// Tests paycheck calculations for different employee scenarios using inline data.
    /// Verifies gross pay, benefit deductions, and net pay calculations.
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [Trait("Priority", "High")]
    [InlineData(
        1, "LeBron", "James", 75420.99, "1984-12-30",
        "LeBron James", 2900.81, 1000.0, 0.0, 0.0, 0.0,
        "" // No dependents - empty string indicates no dependents
    )]
    [InlineData(
        2, "Ja", "Morant", 92365.22, "1999-08-10",
        "Ja Morant", 3552.51, 2953.94, 153.94, 1800.0, 0.0,
        "1,Spouse,Morant,2000-03-03,Spouse|2,Child1,Morant,2020-06-23,Child|3,Child2,Morant,2021-05-18,Child"
    )]
    [InlineData(
        3, "Michael", "Jordan", 143211.12, "1963-02-17",
        "Michael Jordan", 5508.12, 2038.69, 238.69, 600.0, 200.0,
        "4,DP,Jordan,1974-01-02,DomesticPartner"
    )]
    public async Task CalculatePaycheck_VariousEmployeeScenarios_ShouldCalculateCorrectly(
        int employeeId,
        string firstName,
        string lastName,
        decimal salary,
        string dateOfBirthString,
        string expectedEmployeeName,
        decimal expectedGrossPay,
        decimal expectedMonthlyCost,
        decimal expectedHighSalaryCost,
        decimal expectedDependentsCost,
        decimal expectedSeniorCost,
        string dependentsData)
    {
        // Arrange
        var employee = CreateTestEmployeeFromData(
            employeeId, firstName, lastName, salary,
            DateOnly.Parse(dateOfBirthString), dependentsData);
        var employeeDto = MapToEmployeeDto(employee);
        var mockLogger = new Mock<ILogger<EmployeeService>>();
        var mockMediator = new Mock<IMediator>();
        var paycheckCalculationService = CreatePaycheckCalculationService();

        mockMediator.Setup(m => m.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == employeeId), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(employeeDto);

        var service = new EmployeeService(mockMediator.Object, mockLogger.Object, paycheckCalculationService);

        // Act
        var result = await service.GetPaycheckAsync(employeeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(employeeId, result.EmployeeId);
        Assert.Equal(expectedEmployeeName, result.EmployeeName);
        Assert.Equal(expectedGrossPay, result.GrossPay, 2);

        // Verify monthly cost breakdown
        Assert.Equal(expectedMonthlyCost, result.Details.TotalMonthlyCost, 2);
        Assert.Equal(expectedHighSalaryCost, result.Details.HighSalaryAdditionalCost, 2);
        Assert.Equal(expectedDependentsCost, result.Details.DependentsCost, 2);
        Assert.Equal(expectedSeniorCost, result.Details.SeniorDependentsCost, 2);

        // Verify net pay calculation
        var expectedNetPay = result.GrossPay - result.BenefitDeductions;
        Assert.Equal(expectedNetPay, result.NetPay, 2);
    }

    /// <summary>
    /// Tests that error handling works correctly for non-existent employees.
    /// </summary>
    [Theory]
    [Trait("Category", "Unit")]
    [Trait("Feature", "PaycheckCalculation")]
    [Trait("Priority", "Medium")]
    [InlineData(999999)] // Large non-existent ID
    [InlineData(-1)] // Negative ID
    [InlineData(0)] // Zero ID
    [InlineData(100)] // Non-existent but reasonable ID
    public async Task CalculatePaycheck_NonExistentEmployee_ShouldReturnNull(int nonExistentEmployeeId)
    {
        // Arrange
        var mockLogger = new Mock<ILogger<EmployeeService>>();
        var mockMediator = new Mock<IMediator>();
        var paycheckCalculationService = CreatePaycheckCalculationService();

        mockMediator.Setup(m => m.Send(It.Is<GetEmployeePaycheckQuery>(q => q.EmployeeId == nonExistentEmployeeId), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((GetEmployeeDto?)null);

        var service = new EmployeeService(mockMediator.Object, mockLogger.Object, paycheckCalculationService);

        // Act
        var result = await service.GetPaycheckAsync(nonExistentEmployeeId);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Creates test employee from inline data parameters
    /// </summary>
    private static Employee CreateTestEmployeeFromData(
        int employeeId,
        string firstName,
        string lastName,
        decimal salary,
        DateOnly dateOfBirth,
        string dependentsData)
    {
        var dependents = new List<Dependent>();

        if (!string.IsNullOrEmpty(dependentsData))
        {
            var dependentEntries = dependentsData.Split('|');
            foreach (var entry in dependentEntries)
            {
                var parts = entry.Split(',');
                if (parts.Length == 5)
                {
                    var dependent = CreateTestDependent(
                        int.Parse(parts[0]), // ID
                        parts[1], // FirstName
                        parts[2], // LastName
                        DateOnly.Parse(parts[3]), // DateOfBirth
                        Enum.Parse<Relationship>(parts[4]), // Relationship
                        employeeId
                    );
                    dependents.Add(dependent);
                }
            }
        }

        return CreateTestEmployee(employeeId, firstName, lastName, salary, dateOfBirth, dependents);
    }

}
