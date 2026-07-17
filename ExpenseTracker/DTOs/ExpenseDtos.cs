namespace ExpenseTracker.DTOs;

public record CreateExpenseRequest(
    int Id,
    int CategoryId,
    decimal Amount,
    string Currency,
    string Description,
    DateTime ExpenseDate
);

public class ExpenseResponse
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedAt { get; set; }
}