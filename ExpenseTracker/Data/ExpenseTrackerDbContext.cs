using ExpenseTracker.Dto;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options) : DbContext(options)
{
    public DbSet<User> User { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Expense> Expenses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(builder =>
        {
            builder.ToTable("User");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasMaxLength(128);
            builder.Property(e => e.PasswordHash).HasMaxLength(128);
            builder.Property(e => e.Email).HasMaxLength(128);
        });
        
        builder.Entity<Category>(builder =>
        {
            builder.ToTable("Category");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasMaxLength(128);
            builder.Property(e => e.Description).HasMaxLength(128);
        });

        builder.Entity<Expense>(builder =>
        {
            builder.ToTable("Expense");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Name).HasMaxLength(128);
            builder.Property(e => e.Description).HasMaxLength(128);
            builder.Property(e => e.Currency).HasMaxLength(128);
            builder.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId);
            builder.HasOne<User>()
                .WithMany(u => u.ExpenseList)
                .HasForeignKey(e => e.UserId);
        });
    }
}