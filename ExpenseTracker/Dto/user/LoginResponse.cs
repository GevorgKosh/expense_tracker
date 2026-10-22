namespace ExpenseTracker.Dto.user;

public record LoginResponse
{
    public UserResponse User { get; set; }
    public TokenModel Token { get; set; }
}