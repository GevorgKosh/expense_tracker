using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.DTOs.Request.User;

public class UserRegisterRequest
{
   [Required]
   [StringLength(20, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 20 characters")]
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
}