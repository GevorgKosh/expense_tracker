using ExpenseTracker.Dto.category;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

/// <summary>
/// Expense categories (Food, Transportation, ...). Requires a JWT access token.
/// </summary>
[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoryController(ICategoryService service): ControllerBase
{
    /// <summary>
    /// Returns all categories.
    /// </summary>
    /// <param name="includeInactive">When `false`, deactivated categories are left out. Defaults to `true`.</param>
    [HttpGet]
    public async Task<ActionResult<BaseResponse<ICollection<CategoryResponse>>>> GetCategories([FromQuery] bool includeInactive = true)
    {
        var response = await service.GetCategories(includeInactive);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <remarks>
    /// The name must be unique (case-insensitive, surrounding spaces ignored) and each category type can exist only once.
    /// Types: 0 Food, 1 Transportation, 2 Shopping, 3 Utilities, 4 Entertainment.
    /// </remarks>
    /// <param name="category">Type, name and description of the category.</param>
    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> CreateCategory(CategoryRequest category)
    {
        var response = await service.Create(category);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Returns a single category by its id.
    /// </summary>
    /// <param name="id">Category id.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<BaseResponse<CategoryResponse>>> GetByID([FromRoute] int id)
    {
        var response = await service.GetById(id);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Deactivates a category (soft delete).
    /// </summary>
    /// <remarks>
    /// The category and its existing expenses are kept, but no new expenses can be added to it.
    /// Deactivating an already inactive category returns a conflict.
    /// </remarks>
    /// <param name="id">Category id.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory([FromRoute] int id)
    {
        await service.Delete(id);

        return NoContent();
    }

    /// <summary>
    /// Updates the type, name and description of a category.
    /// </summary>
    /// <remarks>
    /// The new name must not match the name of another category (case-insensitive).
    /// </remarks>
    /// <param name="id">Category id.</param>
    /// <param name="category">New type, name and description.</param>
    [HttpPut("{id}")]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory([FromRoute] int id, [FromBody] CategoryRequest category)
    {
        var response = await service.Update(id, category);

        return StatusCode(response.Status, response);
    }
}
