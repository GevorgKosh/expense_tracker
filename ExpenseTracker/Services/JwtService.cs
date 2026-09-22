using ExpenseTracker.Data;

namespace ExpenseTracker.Services;

public class JwtService
{
    public ExpenseDbContext DbContext;
    public IConfiguration Configuration;
    public JwtService(
        ExpenseDbContext dbContext,
        IConfiguration configuration
    )
    {
        Configuration = configuration;
        DbContext = dbContext;
        
    }
    
    
}