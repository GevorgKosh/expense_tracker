using ExpenseTracker.Entities;

namespace ExpenseTracker.DTOs.Response.User;

public record UserResponse(int Id, string Username,  string Email)
{
    public int Id { get; set; } = Id;
    public string Username { get; set; } = Username;
    public string Email { get; set; } = Email;
    public List<Expense>? Expenses { get; set; } =  null;
}