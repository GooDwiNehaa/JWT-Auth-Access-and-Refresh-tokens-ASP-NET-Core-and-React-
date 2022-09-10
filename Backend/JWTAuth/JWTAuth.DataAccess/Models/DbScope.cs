using Microsoft.EntityFrameworkCore;

namespace JWTAuth.DataAccess.Models;

public sealed class DbScope : DbContext
{
    public DbScope(DbContextOptions options) : base(options)
    {
        this.Database.MigrateAsync().GetAwaiter().GetResult();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<UserAgent> UsersAgent { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(u => u.HasIndex(us => us.Email).IsUnique());
    }
}