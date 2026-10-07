namespace ExpenseTracker.Dto;

public class ExpenseRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Double Amount { get; set; }
    public CategoryResponse CategoryRespone { get; set; }
    public int CategoryId { get; set; }
    public CategoryType Type { get; set; }
}