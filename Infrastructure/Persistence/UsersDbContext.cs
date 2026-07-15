using Microsoft.EntityFrameworkCore;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Persistence;

public class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        var e = b.Entity<User>();
        e.ToTable("users");
        e.HasKey(x => x.Id);
        e.HasIndex(x => x.Email).IsUnique();
        e.Property(x => x.Name).HasMaxLength(150).IsRequired();
        e.Property(x => x.Email).HasMaxLength(254).IsRequired();
        e.Property(x => x.PasswordHash).IsRequired();
        e.Property(x => x.Role).HasConversion<string>();
    }
}
