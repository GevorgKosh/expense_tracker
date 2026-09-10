using ExpenseTracker.Dto;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
}