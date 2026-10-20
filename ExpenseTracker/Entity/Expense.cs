namespace ExpenseTracker.Entity;

public class Expense
{
    public  int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Double Amount { get; set; }  //TODO change to decimal
    public DateTime ExpenseDate { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public string Currency { get; set; }
    public Category Category { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
}