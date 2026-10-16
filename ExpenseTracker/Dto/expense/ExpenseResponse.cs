namespace ExpenseTracker.Dto.expense;

public class ExpenseResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Double Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
