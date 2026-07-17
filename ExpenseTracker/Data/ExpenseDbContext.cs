namespace ExpenseTracker.Data;

using ExpenseTracker.Entities;
using Microsoft.EntityFrameworkCore;

public class ExpenseDbContext(DbContextOptions<ExpenseDbContext> options): DbContext(options)
{
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Shopping> Shoppings { get; set; }
    public DbSet<Transportation> Transportations { get; set; }
    public DbSet<Utilities> Utilities { get; set; }
}