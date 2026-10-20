using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;

namespace ExpenseTracker.Service;

public interface ICategoryService
{
    Task<BaseResponse<ICollection<CategoryResponse>>> GetCategories(bool includeInactive);
    Task<BaseResponse<CategoryResponse>> GetById(int id);
    Task<BaseResponse<bool>> Create(CategoryRequest request);
    Task<BaseResponse<bool>> Update(int id, CategoryRequest request);
    Task Delete(int id);
}
