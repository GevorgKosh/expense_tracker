namespace ExpenseTracker.Dto;

public class ExpenseRequest
{
    private string Name { get; set; }
    private string Description { get; set; }
    private Double Amount { get; set; }
    private CategoryResponse CategoryRespone { get; set; }
    private int CategoryId { get; set; }
}