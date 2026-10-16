using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoryController(ICategoryService service): ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BaseResponse<ICollection<CategoryResponse>>>> GetCategories([FromQuery] bool includeInactive = true)
    {
        var response = await service.GetCategories(includeInactive);

        return StatusCode(response.Status, response);
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateCategory(CategoryRequest category)
    {
        var response = await service.Create(category);

        return StatusCode(response.Status, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<CategoryResponse>>> GetByID([FromRoute] int id)
    {
        var response = await service.GetById(id);

        return StatusCode(response.Status, response);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<BaseResponse<bool>>> DeleteCategory([FromRoute] int id)
    {
        var response = await service.Delete(id);

        return StatusCode(response.Status, response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory([FromRoute] int id, [FromBody] CategoryRequest category)
    {
        var response = await service.Update(id, category);

        return StatusCode(response.Status, response);
    }
}