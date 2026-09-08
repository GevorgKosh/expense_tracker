using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/category")]
public class CategoryController(ExpenseDbContext context, IMapper mapper): ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetByID([FromRoute] int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            throw new Exception($"Category with id {id} not found");
        }
        var response = mapper.Map<CategoryResponse>(category);
        
        return Ok(response);
    }
}