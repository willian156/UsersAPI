namespace UsersAPI.Domain.Users;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);
    void Add(User user);
    void Remove(User user);
    Task SaveChangesAsync(CancellationToken ct);
}
