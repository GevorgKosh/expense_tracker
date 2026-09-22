namespace ExpenseTracker.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }
}