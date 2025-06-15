using System;
using Api.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace ApiTests.UnitTests;

/// <summary>
/// Minimal tests for EmployeeService constructor validation.
/// Only tests the paycheckCalculationService parameter which has actual null validation.
/// Service logic is covered by integration tests.
/// </summary>
public class EmployeeServiceTests
{
    [Fact]
    public void Constructor_NullPaycheckCalculationService_ThrowsArgumentNullException()
    {
        var mockMediator = new Mock<IMediator>();
        var mockFeatureManager = new Mock<IFeatureManager>();
        var mockLogger = new Mock<ILogger<EmployeeService>>();

        Assert.Throws<ArgumentNullException>(() => 
            new EmployeeService(mockMediator.Object, mockFeatureManager.Object, mockLogger.Object, null!));
    }
}
