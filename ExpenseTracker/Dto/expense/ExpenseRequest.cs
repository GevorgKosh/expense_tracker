using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dto.expense;

public record ExpenseRequest
{
    [Required] public string? Name { get; set; } 
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public decimal Amount { get; set; }
    [Required] public string Currency { get; set; } = string.Empty;
    [Required] public DateTime ExpenseDate { get; set; }
    [Required] public int CategoryId { get; set; }
}