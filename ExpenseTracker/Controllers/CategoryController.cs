using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/category")]
public class CategoryController(ICategoryService service): ControllerBase
{
    [Authorize]
    [HttpGet("categories")]
    public async Task<ActionResult<BaseResponse<ICollection<CategoryResponse>>>> GetCategories()
    {
        var response = await service.GetCategories();

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateCategory(CategoryRequest category)
    {
        var response = await service.Create(category);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<CategoryResponse>>> GetByID([FromRoute] int id)
    {
        var response = await service.GetById(id);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse<bool>>> DeleteCategory([FromRoute] int id)
    {
        var response = await service.Delete(id);

        return StatusCode(response.Status, response);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory([FromRoute] int id, [FromBody] CategoryRequest category)
    {
        var response = await service.Update(id, category);

        return StatusCode(response.Status, response);
    }
}