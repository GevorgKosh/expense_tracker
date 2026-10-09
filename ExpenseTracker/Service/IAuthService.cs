using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;

namespace ExpenseTracker.Service;

public interface IAuthService
{
    Task<BaseResponse<UserResponse>> Register(UserRegisterRequest request);
    Task<BaseResponse<LoginResponse>> Login(UserLoginRequest request);}