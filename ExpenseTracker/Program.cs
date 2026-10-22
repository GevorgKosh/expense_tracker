using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Exceptions;
using ExpenseTracker.Mapping;
using ExpenseTracker.Middleware;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting ExpenseTracker API");
    DotNetEnv.Env.TraversePath().NoClobber().Load();

    var builder = WebApplication.CreateBuilder(args);
    if (string.IsNullOrWhiteSpace(builder.Configuration["AppSettings:Token"]))
    {
        throw new InvalidOperationException(
            "The JWT signing key is not set. Copy .env.example to .env and fill in AppSettings__Token.");
    }

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    builder.Services.AddValidation();
    builder.Services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
            options.InvalidModelStateResponseFactory = ValidationErrorResponse.Create);
    builder.Services.AddOpenApi();
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml"));
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
    });
    builder.Services.AddDbContext<ExpenseTrackerDbContext>(options => 
        options.UseSqlServer(builder.Configuration.GetConnectionString("UserDatabase")));
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["AppSettings:Issuer"],
                ValidAudience = builder.Configuration["AppSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();
                    if (context.Response.HasStarted) return;

                    var message = context.AuthenticateFailure is SecurityTokenExpiredException
                        ? "Access token has expired"
                        : "Authentication is required to access this resource";

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsJsonAsync(new BaseResponse<object>
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Error = ErrorCodes.Unauthorized,
                        Message = message
                    });
                },
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(new BaseResponse<object>
                    {
                        Status = StatusCodes.Status403Forbidden,
                        Error = ErrorCodes.Forbidden,
                        Message = "You do not have permission to access this resource"
                    });
                }
            };
        });

    builder.Services.AddAuthorization();
    builder.Services.AddScoped<IAuthService,  AuthService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IExpenseService, ExpenseService>();
    builder.Services.AddExceptionHandler<ExpenseExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ExpenseTrackerDbContext>("database");

    var app = builder.Build();
    // Before the exception handler, so the logged status code is the final one (e.g. 404, not 500)
    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ExpenseTrackerDbContext>();
            db.Database.Migrate();
        }

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckResponseWriter.WriteJson
    });
    app.Run();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    // HostAbortedException is thrown on purpose by EF Core tools and WebApplicationFactory, so it isn't a failure
    Log.Fatal(exception, "ExpenseTracker API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
