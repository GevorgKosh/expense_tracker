namespace ExpenseTracker.Entities;

public class Category
{
    public int ID { get; set; }
    public string Name { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Expense> Expenses { get; set; }
}