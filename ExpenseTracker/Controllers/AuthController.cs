using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController: ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(ExpenseTrackerDbContext context, IAuthService service, UserRegisterRequest request)
    {
        var user = await service.Register(context, request);
        if (user is null)
        {
            return BadRequest("Username already exists");
        }
        return Ok(user);
    }
}