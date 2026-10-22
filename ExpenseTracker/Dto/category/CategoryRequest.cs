using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Entity;

namespace ExpenseTracker.Dto.category;

public record CategoryRequest
{
    [Required] public CategoryType Type { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
}