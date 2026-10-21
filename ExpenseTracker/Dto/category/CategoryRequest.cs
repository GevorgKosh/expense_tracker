using ExpenseTracker.Entity;

namespace ExpenseTracker.Dto.category;

public class CategoryRequest
{
    public CategoryType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}