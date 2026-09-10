namespace ExpenseTracker.Dto;

public class CategoryResponse
{
    private int Id { get; set; }
    private int Type { get; set; }
    private string Name { get; set; }
    private string Description { get; set; }
    private List<ExpenseResponse> Expenses { get; set; }
}