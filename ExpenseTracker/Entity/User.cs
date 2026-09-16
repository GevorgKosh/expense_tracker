namespace ExpenseTracker.Dto;

public class User
{
    public User()
    {}
    public User(string? userName, string? passwordHash)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }
    
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Expense>? ExpenseList { get; set; } = null;
}