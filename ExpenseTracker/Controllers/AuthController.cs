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
    public async Task<ActionResult<User>> Register(UserRegisterRequest request)
    {
        var user = await service.Register(request);
        if (user is null)
        {
            return BadRequest("Username already exists");
        }
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UserLoginRequest request)
    {
        var token = await service.Login(request);
        if (token is null)
        {
            return BadRequest("Username or password is incorrect");
        }
        return Ok(token);
    }
}