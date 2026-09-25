namespace ExpenseTracker.Dto.user;

public record UserResponse(
    int Id,
    string UserName,
    string Email,
    List<Expense>? Expenses = null
    );