using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;

namespace ExpenseTracker.Service;

public interface IAuthService
{
    Task<User?> Register(ExpenseTrackerDbContext context, UserRegisterRequest request);
    Task<User?> Login(ExpenseTrackerDbContext context, UserLoginRequest request);
}