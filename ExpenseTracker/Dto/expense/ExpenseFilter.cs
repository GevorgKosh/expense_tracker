namespace ExpenseTracker.Dto.expense;

public class ExpenseFilter
{
    public const int MaxPageSize = 100;

    public int? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Currency { get; set; }
    public decimal? MinimumAmount { get; set; }
    public decimal? MaximumAmount { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "expenseDate";
    public string SortDirection { get; set; } = "desc";
}
