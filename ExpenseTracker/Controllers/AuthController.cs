using ExpenseTracker.Dto.user;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

/// <summary>
/// User registration and login.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService service): ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// The user name must be unique. The password must be 3 to 50 characters long.
    /// </remarks>
    /// <param name="request">User name, password and email of the new user.</param>
    [HttpPost("register")]
    public async Task<ActionResult<BaseResponse<UserResponse>>> Register(UserRegisterRequest request)
    {
        var response = await service.Register(request);

        return StatusCode(response.Status, response);
    }

    /// <summary>
    /// Logs a user in and returns a JWT access token and a refresh token.
    /// </summary>
    /// <remarks>
    /// Copy `details.token.accessToken` from the response into the **Authorize** button to call the other endpoints.
    /// The access token is valid for 15 minutes.
    /// </remarks>
    /// <param name="request">User name and password.</param>
    [HttpPost("login")]
    public async Task<ActionResult<BaseResponse<LoginResponse>>> Login(UserLoginRequest request)
    {
        var response = await service.Login(request);

        return StatusCode(response.Status, response);
    }
}
