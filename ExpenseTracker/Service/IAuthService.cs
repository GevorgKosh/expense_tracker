using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;

namespace ExpenseTracker.Service;

public interface IAuthService
{
    Task<User?> Register(UserRegisterRequest request);
    Task<string?> Login(UserLoginRequest request);
}