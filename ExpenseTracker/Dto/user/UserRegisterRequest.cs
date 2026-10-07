using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dto.user;

public record UserRegisterRequest
{
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = string.Empty;
    [Required(ErrorMessage = "Password is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Password must be between 3 and 50 characters")]
    public string Password { get; set; } = string.Empty;
    [Required(ErrorMessage = "ConfirmPassword is required")]
    [RegularExpression("[A-Z0-9a-z._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}", ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}