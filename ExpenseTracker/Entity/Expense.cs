namespace ExpenseTracker.Dto;

public class Expense
{
    public  int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Double Amount { get; set; }
    public DateTime Date { get; set; }
    public string Currency { get; set; }
    public Category Category { get; set; }
    public int CategoryId { get; set; }
}