using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/category")]
public class CategoryController(ExpenseDbContext context, IMapper mapper): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ICollection<CategoryResponse>>> GetCategories()
    {
        var response = await context.Categories.ToListAsync();
        var categories = mapper.Map<ICollection<CategoryResponse>>(response);
        return Ok(categories);
    }

    [HttpPost]
    public async Task<ActionResult<Boolean>> CreateCategory(CategoryRequest category)
    {
        var newCategory = mapper.Map<Category>(category);
        context.Categories.Add(newCategory);
        
        var result = await context.SaveChangesAsync();
        
        return Ok(result > 0);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetByID([FromRoute] int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            throw new Exception($"Category with id {id} not found");
        }
        var response = mapper.Map<CategoryResponse>(category);
        
        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory([FromRoute] int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
            throw new Exception($"Category with id {id} not found");

        context.Categories.Remove(category);
        var result = await context.SaveChangesAsync();
        
        return Ok(result > 0);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory([FromRoute] int id, [FromBody] CategoryRequest category)
    {
        var updatedCategory = await context.Categories.FindAsync(id);
        if (updatedCategory is null)
            throw new Exception($"Category with id {id} not found");
        
        context.Categories.Update(updatedCategory);
        var result = await context.SaveChangesAsync();
        return Ok(result > 0);
    }
}