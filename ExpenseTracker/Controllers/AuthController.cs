using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service): ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<UserResponse>>> Register(UserRegisterRequest request)
    {
        var response = await service.Register(request);

        return StatusCode(response.Status, response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<LoginResponse>>> Login(UserLoginRequest request)
    {
        var response = await service.Login(request);

        return StatusCode(response.Status, response);
    }
}