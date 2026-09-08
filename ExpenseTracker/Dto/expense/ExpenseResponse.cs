namespace ExpenseTracker.Dto;

public class ExpenseResponse
{
    public  int Id { get; set; }
    public string Name { get; set; }
    public Double Amount { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public string Currency { get; set; }
    public CategoryResponse CategoryRespone { get; set; }
    public int CategoryId { get; set; }
}