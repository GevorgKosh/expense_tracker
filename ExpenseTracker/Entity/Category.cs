namespace ExpenseTracker.Dto;

public class Category
{
    public int Id { get; set; }
    public CategoryType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Expense> Expenses { get; set; }
}