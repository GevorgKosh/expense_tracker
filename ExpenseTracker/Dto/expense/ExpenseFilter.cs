namespace ExpenseTracker.Dto;

public class ExpenseFilter
{
    public int? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Currency { get; set; }
    public Double? MinAmount { get; set; }
    public Double? MaxAmount { get; set; }
}