namespace ExpenseTracker.Dto;

public class CategoryResponse
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ExpenseResponse> Expenses { get; set; }
}