namespace ExpenseTracker.Dto.user;

public class LoginResponse
{
    public UserResponse User { get; set; }
    public TokenModel Token { get; set; }
}