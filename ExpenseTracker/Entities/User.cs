using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Entities;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Username { get; set; }

    public string HashPassword { get; set; } = string.Empty;
    public string Email { get; set; }
    public List<Expense>? Expenses  { get; set; }
    public RefreshToken RefreshToken { get; set; }

    public User(string username, string hashPassword, string email)
    {
        Username = username;
        HashPassword = hashPassword;
        Email = email;
    }
}