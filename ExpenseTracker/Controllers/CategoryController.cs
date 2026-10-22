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
[Produces("application/json")]
// Every action requires a token, so every action can return 401
[ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
public class CategoryController(ICategoryService service): ControllerBase
{
    /// <summary>
    /// Returns all categories.
    /// </summary>
    /// <param name="includeInactive">When `false`, deactivated categories are left out. Defaults to `true`.</param>
    /// <response code="200">The list of categories.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    [HttpGet]
    [ProducesResponseType(typeof(BaseResponse<ICollection<CategoryResponse>>), StatusCodes.Status200OK)]
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
    /// <response code="201">The category was created.</response>
    /// <response code="400">The request body is invalid (e.g. unknown category type).</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="409">A category with this name or type already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResponse<bool>>> CreateCategory(CategoryRequest category)
    {
        var response = await service.Create(category);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Returns a single category by its id.
    /// </summary>
    /// <param name="id">Category id.</param>
    /// <response code="200">The category.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">No category with this id exists.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BaseResponse<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
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
    /// <response code="204">The category was deactivated.</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">No category with this id exists.</response>
    /// <response code="409">The category is already inactive.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
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
    /// <response code="200">The category was updated.</response>
    /// <response code="400">The request body is invalid (e.g. unknown category type).</response>
    /// <response code="401">The access token is missing, invalid or expired.</response>
    /// <response code="404">No category with this id exists.</response>
    /// <response code="409">Another category already has this name.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BaseResponse<bool>>> UpdateCategory([FromRoute] int id, [FromBody] CategoryRequest category)
    {
        var response = await service.Update(id, category);

        return StatusCode(response.Status, response);
    }
}
