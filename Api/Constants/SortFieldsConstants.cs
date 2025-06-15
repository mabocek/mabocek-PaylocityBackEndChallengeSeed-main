namespace Api.Constants;

/// <summary>
/// Centralized constants for sorting fields across the application
/// </summary>
public static class SortFieldsConstants
{
    /// <summary>
    /// Common sorting fields used across entities
    /// </summary>
    public static class Common
    {
        public const string FirstName = "firstname";
        public const string LastName = "lastname";
        public const string DateOfBirth = "dateofbirth";
    }

    /// <summary>
    /// Employee-specific sorting fields
    /// </summary>
    public static class Employee
    {
        public const string FirstName = Common.FirstName;
        public const string LastName = Common.LastName;
        public const string DateOfBirth = Common.DateOfBirth;
        public const string Salary = "salary";
    }

    /// <summary>
    /// Dependent-specific sorting fields
    /// </summary>
    public static class Dependent
    {
        public const string FirstName = Common.FirstName;
        public const string LastName = Common.LastName;
        public const string DateOfBirth = Common.DateOfBirth;
        public const string Relationship = "relationship";
    }
}

/// <summary>
/// Constants for API responses and error messages
/// </summary>
public static class ApiConstants
{
    public static class ErrorMessages
    {
        public const string NotFound = "Resource not found";
        public const string BadRequest = "Invalid request";
        public const string InternalServerError = "An error occurred while processing your request";
    }

    public static class SuccessMessages
    {
        public const string Created = "Resource created successfully";
        public const string Updated = "Resource updated successfully";
        public const string Deleted = "Resource deleted successfully";
    }
}
