namespace ExpenseTracker.Dto.expense;

public class ExpenseFilter
{
    public int? CategoryId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Currency { get; set; }
    public Double? MinimumAmount { get; set; }
    public Double? MaximumAmount { get; set; }
}