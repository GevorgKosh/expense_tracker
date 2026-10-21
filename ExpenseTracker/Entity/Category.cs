namespace ExpenseTracker.Entity;

public class Category
{
    public int Id { get; set; }
    public CategoryType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Expense> Expenses { get; set; }
}