using Api.Models;

namespace Api.Dtos.Paycheck;

/// <summary>
/// DTO showing the detailed cost breakdown for a specific dependent.
/// Provides transparency into how each dependent's benefit costs are calculated.
/// </summary>
public record DependentCostBreakdownDto(
    /// <summary>
    /// The ID of the dependent
    /// </summary>
    int DependentId,

    /// <summary>
    /// Full name of the dependent for display purposes
    /// </summary>
    string? DependentName,

    /// <summary>
    /// Relationship of the dependent to the employee (Spouse, DomesticPartner, Child)
    /// Based on requirement: "an employee may only have 1 spouse or domestic partner (not both)"
    /// and "an employee may have an unlimited number of children"
    /// </summary>
    Relationship Relationship,

    /// <summary>
    /// Current age of the dependent (calculated from DateOfBirth)
    /// Used to determine if the dependent qualifies for the over-50 additional cost
    /// </summary>
    int Age,

    /// <summary>
    /// Base monthly cost for this dependent
    /// Based on requirement: "each dependent represents an additional $600 cost per month (for benefits)"
    /// </summary>
    decimal BaseCost,

    /// <summary>
    /// Additional monthly cost if dependent is in the senior category
    /// Based on requirement: "dependents that are over 50 years old will incur an additional $200 per month"
    /// Will be $0 if dependent is 50 or younger
    /// </summary>
    decimal SeniorAdditionalCost,

    /// <summary>
    /// Total monthly cost for this dependent (BaseCost + SeniorAdditionalCost)
    /// </summary>
    decimal TotalCost
);
