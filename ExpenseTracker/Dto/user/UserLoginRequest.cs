using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Dto.user;

public record UserLoginRequest
(
    [Required] string UserName,
    [Required] string PasswordHash
);