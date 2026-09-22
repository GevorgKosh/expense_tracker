using ExpenseTracker.Data;
using ExpenseTracker.DTOs;
using ExpenseTracker.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api")]
public class CategoryController(ExpenseDbContext db): ControllerBase
{
    
    [HttpGet("categories")]
    public async Task<ActionResult<List<Category>>> GetAllCategories([FromQuery] bool includeInactive = true)
    {
        IQueryable<Category> query = db.Categories;
        if (!includeInactive)
        {
            query = query.Where(category => category.IsActive);
        }

        var categories = await query.ToListAsync();
        return Ok(categories);
    }    
    
    [HttpGet("categories/{id}")]
    public async Task<ActionResult<Category>> GetAllCategories(int id)
    {
        var category = await db.Categories.FirstOrDefaultAsync(category => category.ID == id);
        if (category == null)
        {
            return NotFound(new BaseApiResponse<Category>(statusCode: 404, errorMessage: "Category not found"));
        }
        else
        { 
            return Ok(new BaseApiResponse<Category>(statusCode: 200, message: "Success", details: category));   
        }
    }    
    
}