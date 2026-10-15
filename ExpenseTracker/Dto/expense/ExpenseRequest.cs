namespace ExpenseTracker.Dto;

public class ExpenseRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public CategoryResponse? ExpenseDate { get; set; }
    public int CategoryId { get; set; }
}