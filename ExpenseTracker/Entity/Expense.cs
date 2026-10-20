namespace ExpenseTracker.Entity;

public class Expense
{
    public  int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Category Category { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
}