using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseTracker.Service;

public class AuthService(ExpenseTrackerDbContext context, IConfiguration configuration): IAuthService
{
    public async Task<User?> Register(UserRegisterRequest request)
    {
        var isExist = context.Users.Any(u => u.UserName == request.UserName);
        if (!isExist) throw new Exception("User with such name is already exists");
        
        var user = new User();
 
        var hasher = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.UserName = request.UserName;
        user.PasswordHash = hasher;
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user;
    }

    public async Task<string?> Login(UserLoginRequest request)
    {
        var hasher = new PasswordHasher<User>();
        var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash) == PasswordVerificationResult.Failed)
            throw new Exception("Incorrect username or password");

        string token = CreateToken(user);

        return token;
    }

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: DateTime.Now.AddHours(3),
            signingCredentials: creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}