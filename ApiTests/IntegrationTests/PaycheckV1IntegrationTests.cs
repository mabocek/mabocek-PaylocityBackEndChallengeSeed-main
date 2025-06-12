using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Api.Dtos.Dependent;
using Api.Dtos.Employee;
using Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ApiTests.IntegrationTests;

/// <summary>
/// Integration tests for the paycheck calculation endpoint.
/// Simplified tests focusing on end-to-end API functionality.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Feature", "PaycheckCalculation")]
public class PaycheckV1IntegrationTests : IntegrationTestBase
{
    public PaycheckV1IntegrationTests(WebApplicationFactory<Program> factory) : base(factory)
    {
    }

    /// <summary>
    /// Tests paycheck endpoint for various employee scenarios.
    /// Verifies API returns valid paycheck data with correct structure.
    /// </summary>
    [Theory]
    [Trait("TestType", "GetPaycheck")]
    [Trait("Priority", "High")]
    [InlineData(1, "LeBron James", false, false)] // No dependents, under $80K
    [InlineData(2, "Ja Morant", true, true)]     // Has dependents, over $80K
    [InlineData(3, "Michael Jordan", true, true)] // Has over-50 dependent, over $80K
    public async Task GetPaycheck_ValidEmployee_ShouldReturnCorrectStructure(
        int employeeId,
        string expectedName,
        bool shouldHaveDependents,
        bool shouldHaveHighSalaryCost)
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/v1/employees/{employeeId}/paycheck");

        // Assert - Basic response validation
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains(expectedName, content);

        // Verify required paycheck structure exists
        AssertPaycheckStructure(content);

        // Verify business logic elements based on employee
        if (shouldHaveDependents)
        {
            Assert.Contains("dependentBreakdowns", content);
            Assert.Contains("dependentsCost", content);
        }

        if (shouldHaveHighSalaryCost)
        {
            Assert.Contains("highSalaryAdditionalCost", content);
        }

        // Verify 26 paychecks per year requirement
        Assert.Contains("\"paychecksPerYear\":26", content.Replace(" ", ""));
    }

    /// <summary>
    /// Tests error handling for non-existent employee.
    /// </summary>
    [Fact]
    [Trait("TestType", "ErrorHandling")]
    [Trait("Priority", "Medium")]
    public async Task GetPaycheck_NonExistentEmployee_ShouldReturn404()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/v1/employees/999999/paycheck");

        // Assert
        await response.ShouldReturn(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Helper method to verify paycheck JSON structure.
    /// </summary>
    private static void AssertPaycheckStructure(string content)
    {
        // Verify main paycheck properties
        Assert.Contains("grossPay", content);
        Assert.Contains("benefitDeductions", content);
        Assert.Contains("netPay", content);
        Assert.Contains("details", content);

        // Verify details structure
        Assert.Contains("employeeBaseCost", content);
        Assert.Contains("totalMonthlyCost", content);
        Assert.Contains("perPaycheckDeduction", content);
    }
}
