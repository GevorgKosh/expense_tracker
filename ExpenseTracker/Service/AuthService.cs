using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    public async Task<BaseResponse<UserResponse>> Register(UserRegisterRequest request)
    {
        var isExist = await context.User.AnyAsync(u => u.Name == request.UserName);
        if (isExist)
        {
            return new BaseResponse<UserResponse>
            {
                Status = StatusCodes.Status409Conflict,
                Error = "Conflict",
                Message = "User with such name already exists"
            };
        }

        var user = new User();
 
        var hasher = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.Name = request.UserName;
        user.PasswordHash = hasher;
        user.Email = request.Email;
        
        context.User.Add(user);
        await context.SaveChangesAsync();
        
        return new BaseResponse<UserResponse>
        {
            Status = StatusCodes.Status201Created,
            Message = "User registered successfully",
            Details = new UserResponse(user.Id, user.Name, user.Email)
        };
    }

    public async Task<BaseResponse<LoginResponse>> Login(UserLoginRequest request)
    {
        var hasher = new PasswordHasher<User>();
        var user = await context.User.FirstOrDefaultAsync(u => u.Name == request.UserName);
        if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash) == PasswordVerificationResult.Failed)
        {
            return new BaseResponse<LoginResponse>
            {
                Status = StatusCodes.Status401Unauthorized,
                Error = "Unauthorized",
                Message = "Incorrect username or password"
            };
        }

        var token = await CreateTokenModel(user);

        return new BaseResponse<LoginResponse>
        {
            Status = StatusCodes.Status200OK,
            Message = "Logged in successfully",
            Details = new LoginResponse
            {
                User = new UserResponse(user.Id, user.Name, user.Email),
                Token = token
            }
        };
    }

    // Issues a new access/refresh token pair and stores the refresh token on the user.
    private async Task<TokenModel> CreateTokenModel(User user)
    {
        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now + TokenModel.AccessTokenLifetime;
        var refreshTokenExpiresAt = now + TokenModel.RefreshTokenLifetime;

        user.RefreshToken = GenerateRefreshToken();
        user.RefreshTokenExpiresAt = refreshTokenExpiresAt;
        await context.SaveChangesAsync();

        return new TokenModel
        {
            AccessToken = CreateAccessToken(user, accessTokenExpiresAt),
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    private string CreateAccessToken(User user, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var tokenDescriptor = new JwtSecurityToken(
            issuer: configuration.GetValue<string>("AppSettings:Issuer"),
            audience: configuration.GetValue<string>("AppSettings:Audience"),
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}