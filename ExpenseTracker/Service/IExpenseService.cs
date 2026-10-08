using ExpenseTracker.Dto;

namespace ExpenseTracker.Service;

public interface IExpenseService
{
    Task<BaseResponse<ICollection<ExpenseResponse>>> GetExpenses();
    Task<BaseResponse<ExpenseResponse>> GetById(int id);
    Task<BaseResponse<ICollection<ExpenseResponse>>> GetFiltered(ExpenseFilter filter);
    Task<BaseResponse<bool>> Create(ExpenseRequest request);
    Task<BaseResponse<bool>> Update(int id, ExpenseRequest request);
    Task<BaseResponse<bool>> Delete(int id);
}
