using AutoMapper;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;
using ExpenseTracker.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Service;

public class CategoryService(ExpenseTrackerDbContext context, IMapper mapper): ICategoryService
{
    public async Task<BaseResponse<ICollection<CategoryResponse>>> GetCategories(bool includeInactive)
    {
        var categories = await context.Categories.Where(c => includeInactive || c.IsActive).ToListAsync();

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
            throw new NotFoundException($"Category with id {id} not found");
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
            throw new ConflictException("Category already exist");
        }

        var isTypeAlreadyExist = await context.Categories.AnyAsync(c => c.Type ==  request.Type);
        if (isTypeAlreadyExist)
        {
            throw new ConflictException("Category type already exist");
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
            throw new NotFoundException($"Category with id {id} not found");
        }

        var restrictUpdating = await context.Categories.AnyAsync(c => c.Id != id && c.Name.Trim().ToLower() == request.Name.Trim().ToLower());
        if (restrictUpdating)
        {
            throw new ConflictException("Category with such name is already exist");
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

    public async Task Delete(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is null)
        {
            throw new NotFoundException($"Category with id {id} not found");
        }

        if (!category.IsActive)
        {
            throw new ConflictException("Category is already inactive");
        }

        category.IsActive = false;

        context.Categories.Update(category);
        await context.SaveChangesAsync();
    }
}
