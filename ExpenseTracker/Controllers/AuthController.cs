using ExpenseTracker.Dto.user;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers;

/// <summary>
/// User registration and login.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(IAuthService service): ControllerBase
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// The user name must be unique. The password must be 3 to 50 characters long.
    /// </remarks>
    /// <param name="request">User name, password and email of the new user.</param>
    /// <response code="201">The user was registered.</response>
    /// <response code="400">A field is missing or invalid (e.g. bad email format or password length).</response>
    /// <response code="409">A user with this name already exists.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(BaseResponse<UserResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
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
    /// <response code="200">Logged in; the response contains the tokens.</response>
    /// <response code="400">User name or password is missing.</response>
    /// <response code="401">The user name or password is incorrect.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(BaseResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<IDictionary<string, string[]>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BaseResponse<LoginResponse>>> Login(UserLoginRequest request)
    {
        var response = await service.Login(request);

        return StatusCode(response.Status, response);
    }
}
