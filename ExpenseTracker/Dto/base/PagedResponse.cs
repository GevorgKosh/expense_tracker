namespace ExpenseTracker.Dto;

public class PagedResponse<T>
{
    public ICollection<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
