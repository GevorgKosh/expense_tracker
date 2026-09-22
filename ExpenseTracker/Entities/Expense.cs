namespace ExpenseTracker.Entities;

public class Expense
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public required Category Category { get; set; }  //take attention to nullable, also required
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string Description { get; set; }
    public DateTime ExpenseDate { get; set; }
}