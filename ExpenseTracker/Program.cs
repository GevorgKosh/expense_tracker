using ExpenseTracker.Data;
using ExpenseTracker.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
//                   ?? throw new InvalidOperationException("Jwt configuration section is missing.");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllers();
//builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ExpenseDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//         {
//             options.TokenValidationParameters = new TokenValidationParameters
//             {
//                 ValidateIssuer = true,
//                 ValidateAudience = true,
//                 ValidateLifetime = true,
//                 ValidateIssuerSigningKey = true,
//
//                 ValidIssuer = jwtSettings.Issuer,
//                 ValidAudience = jwtSettings.Audience,
//                 IssuerSigningKey = new SymmetricSecurityKey(
//                     Encoding.UTF8.GetBytes(jwtSettings.Secret)
//                     ),
//                 ClockSkew = TimeSpan.FromSeconds(30)
//             };
//         }
//     );
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
 //   app.MapOpenApi();
}

app.UseMiddleware<ExpenseTrackerResponseMiddleware>();   // ամենավերևում — բռնում է ամեն exception

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();