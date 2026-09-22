using ExpenseTracker.Data;
using ExpenseTracker.DTOs.Request.User;
using ExpenseTracker.DTOs.Response.User;
using ExpenseTracker.Entities;
using ExpenseTracker.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class AuthController(ExpenseDbContext dbContext): ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(UserRegisterRequest request)
    {
        var userExists = await dbContext.Users.AnyAsync(u => u.Email == request.Email);
        if (userExists)
        {
           throw new ConflictException($"Email {request.Email} is already registered");
        }
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            username: request.Username,
            hashPassword: passwordHash,
            email: request.Email
        );
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var userResponse = new UserResponse(Id: user.Id, user.Username, user.Email);

        return CreatedAtAction(nameof(Register), new { id = user.Id }, userResponse);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserResponse>> Login(UserLoginRequest request)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.HashPassword))
        {
            throw new ValidationException("Invalid credentials");
        }

        var userResponse = new UserResponse(Id: user.Id, user.Username, user.Email);
        return Ok(userResponse);
    }
}