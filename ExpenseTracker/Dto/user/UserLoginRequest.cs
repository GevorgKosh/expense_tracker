namespace ExpenseTracker.Dto.user;

public class UserLoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}