using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.Dto;

[Table("User")]
public class User
{
    public User()
    {}
    public User(string userName, string passwordHash)
    {
        UserName = userName;
        PasswordHash = passwordHash;
    }

    public User(int id, string userName, string email, string passwordHash)
    {
        Id = id;
        UserName = userName;
        Email = email;
        PasswordHash = passwordHash;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Expense>? ExpenseList { get; set; } = null;
}