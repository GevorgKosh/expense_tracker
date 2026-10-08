using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Service;

public class CategoryService(ExpenseTrackerDbContext context, IMapper mapper): ICategoryService
{
    public async Task<BaseResponse<ICollection<CategoryResponse>>> GetCategories()
    {
        var categories = await context.Categories.ToListAsync();

        return new BaseResponse<ICollection<CategoryResponse>>
        {
            Status = StatusCodes.Status200OK,
            Message = "Categories retrieved successfully",
            Details = mapper.Map<ICollection<CategoryResponse>>(categories)
        };
    }

    public async Task<BaseResponse<CategoryResponse>> GetById(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound<CategoryResponse>(id);
        }

        return new BaseResponse<CategoryResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Category retrieved successfully",
            Details = mapper.Map<CategoryResponse>(category)
        };
    }

    public async Task<BaseResponse<bool>> Create(CategoryRequest request)
    {
        var category = mapper.Map<Category>(request);
        context.Categories.Add(category);
        var result = await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status201Created,
            Message = "Category created successfully",
            Details = result > 0
        };
    }

    public async Task<BaseResponse<bool>> Update(int id, CategoryRequest request)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound<bool>(id);
        }

        mapper.Map(request, category);
        await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Category updated successfully",
            Details = true
        };
    }

    public async Task<BaseResponse<bool>> Delete(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound<bool>(id);
        }

        context.Categories.Remove(category);
        var result = await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Category deleted successfully",
            Details = result > 0
        };
    }

    private static BaseResponse<T> NotFound<T>(int id)
    {
        return new BaseResponse<T>
        {
            Status = StatusCodes.Status404NotFound,
            Error = "Not Found",
            Message = $"Category with id {id} not found"
        };
    }
}
