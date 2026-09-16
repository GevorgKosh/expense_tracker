using ExpenseTracker.Data;
using ExpenseTracker.Dto;
using ExpenseTracker.Dto.user;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Service;

public class AuthService: IAuthService
{
    public async Task<User?> Register(ExpenseTrackerDbContext context, UserRegisterRequest request)
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

    public async Task<User?> Login(ExpenseTrackerDbContext context, UserLoginRequest request)
    {
        var user = await context.Users.SingleOrDefaultAsync(u => u.UserName == request.UserName);
        if (user is null || PasswordHasher)
            throw new Exception("Incorrect username or password");
 
        var hash = new PasswordHasher<User>().HashPassword(user, request.Password);
        user.UserName = request.UserName;
        user.PasswordHash = hash;
        
        context.Users.Add(user);
        await context.SaveChangesAsync();
        
        return user;
    }
}