namespace ExpenseTracker.Data;

using ExpenseTracker.Entities;
using Microsoft.EntityFrameworkCore;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options): DbContext(options)
{
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<User> Users { get; set; }
}