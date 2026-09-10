namespace ExpenseTracker.Dto.category;

public class CategoryRequest
{
    public CategoryType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}