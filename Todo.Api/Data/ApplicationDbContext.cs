using Microsoft.EntityFrameworkCore;
using Todo.Api.Models;

namespace Todo.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<TodoTask> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoTask>(entity =>
        {
            // Add index on DueDate for efficient date range queries
            entity.HasIndex(e => e.DueDate)
                .HasDatabaseName("IX_Todos_DueDate");

            // Add composite index for completion percentage and due date queries
            entity.HasIndex(e => new { e.CompletionPercentage, e.DueDate })
                .HasDatabaseName("IX_Todos_CompletionPercentage_DueDate");
        });
    }
}