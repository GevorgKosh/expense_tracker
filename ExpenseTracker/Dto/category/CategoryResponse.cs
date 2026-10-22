using ExpenseTracker.Dto.expense;

namespace ExpenseTracker.Dto.category;

public record CategoryResponse
{
    public int Id { get; set; }
    public int Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<ExpenseResponse> Expenses { get; set; } = new ();
}