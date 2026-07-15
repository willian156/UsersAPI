using Microsoft.EntityFrameworkCore;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Persistence;

public class UserRepository(UsersDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) => db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) => db.Users.FirstOrDefaultAsync(x => x.Email == email, ct);
    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct) => await db.Users.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
    public void Add(User user) => db.Users.Add(user);
    public void Remove(User user) => db.Users.Remove(user);
    public async Task SaveChangesAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
}
