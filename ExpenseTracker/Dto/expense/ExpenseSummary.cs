using ExpenseTracker.Dto.category;

namespace ExpenseTracker.Dto.expense;

public class ExpenseSummary
{
    public DateOnly DateFrom { get; set; }
    public DateOnly DateTo { get; set; }
    public int ExpenseCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Currency { get; set; }
    public ICollection<CategorySummary> Categories { get; set; } = new List<CategorySummary>();
}