using ExpenseTracker.Dto.expense;

namespace ExpenseTracker.Service;

public interface IExpenseService
{
    Task<BaseResponse<ICollection<ExpenseResponse>>> GetExpenses(int userId, ExpenseFilter filter);
    Task<BaseResponse<ExpenseResponse>> GetById(int userId, int id);
    Task<BaseResponse<ExpenseSummary>> GetSummary(int userId, DateOnly? dateFrom, DateOnly? dateTo);
    Task<BaseResponse<bool>> Create(int userId, ExpenseRequest request);
    Task<BaseResponse<bool>> Update(int userId, int id, ExpenseRequest request);
    Task Delete(int userId, int id);
}
