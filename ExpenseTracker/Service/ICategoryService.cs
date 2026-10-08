using ExpenseTracker.Dto;
using ExpenseTracker.Dto.category;

namespace ExpenseTracker.Service;

public interface ICategoryService
{
    Task<BaseResponse<ICollection<CategoryResponse>>> GetCategories();
    Task<BaseResponse<CategoryResponse>> GetById(int id);
    Task<BaseResponse<bool>> Create(CategoryRequest request);
    Task<BaseResponse<bool>> Update(int id, CategoryRequest request);
    Task<BaseResponse<bool>> Delete(int id);
}
