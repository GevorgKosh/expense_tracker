using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.DTOs;

public record CreateCategoryRequest  //take attention to record
{
    public required string Name { get; set; }
    public required string Description { get; set; }
}