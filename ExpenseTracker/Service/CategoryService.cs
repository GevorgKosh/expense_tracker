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
        var isNameAlreadyExist = await context.Categories.AnyAsync(c => c.Name.Trim().ToLower() == request.Name.Trim().ToLower());
        if (isNameAlreadyExist)
        {
            return new BaseResponse<bool>
            {
                Status = StatusCodes.Status409Conflict,
                Message = "Category already exist",
                Details = false
            };
        }
        
        var isTypeAlreadyExist = await context.Categories.AnyAsync(c => c.Type ==  request.Type);
        if (isTypeAlreadyExist)
        {
            return new BaseResponse<bool>
            {
                Status = StatusCodes.Status409Conflict,
                Message = "Category type already exist",
                Details = false
            };
        }
        
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
            return new BaseResponse<bool>
            {
                Status = StatusCodes.Status404NotFound,
                Message = "Category isn`t found",
                Details = false
            };
        }
        
        var restrictUpdating = await context.Categories.AnyAsync(c => c.Id != id && c.Name.Trim().ToLower() == request.Name.Trim().ToLower());
        if (restrictUpdating)
        {
            return new BaseResponse<bool>
            {
                Status = StatusCodes.Status409Conflict,
                Message = "Category with such name is already exist",
                Details = false
            };
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

        if (!category.IsActive)
        {
            return new BaseResponse<bool>
            {
                Status = StatusCodes.Status409Conflict,
                Message = "Category is already inactive",
                Details = false
            };
        }

        category.IsActive = false;

        context.Categories.Update(category);
        var result = await context.SaveChangesAsync();

        return new BaseResponse<bool>
        {
            Status = StatusCodes.Status200OK,
            Message = "Category deleted successfully",
            Details = true
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
