using System.ComponentModel.DataAnnotations;

namespace Api.Models;

/// <summary>
/// Represents a paginated result set with metadata about pagination
/// </summary>
/// <typeparam name="T">The type of items in the result set</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items for the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Creates a new paginated result
    /// </summary>
    /// <param name="items">The items for the current page</param>
    /// <param name="totalItems">Total number of items across all pages</param>
    /// <param name="currentPage">Current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    public PagedResult(List<T> items, int totalItems, int currentPage, int pageSize)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        TotalItems = totalItems;
        CurrentPage = Math.Max(1, currentPage); // Ensure page is at least 1
        PageSize = Math.Max(1, pageSize); // Ensure page size is at least 1
        TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
    }

    /// <summary>
    /// Creates an empty paginated result
    /// </summary>
    public PagedResult()
    {
        CurrentPage = 1;
        PageSize = 10;
        TotalPages = 0;
        TotalItems = 0;
    }
}

/// <summary>
/// Pagination parameters for API requests
/// </summary>
public class PaginationParameters
{
    private int _pageSize = 10;
    private const int MaxPageSize = 100;

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page (max 100)
    /// </summary>
    [Range(1, MaxPageSize, ErrorMessage = "Page size must be between 1 and 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
