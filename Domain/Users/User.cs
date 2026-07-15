namespace UsersAPI.Domain.Users;

public class User
{
    private User()
    {
    }

    private User(Guid id, string name, string email, string passwordHash, UserRole role)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static User Create(string name, string email, string passwordHash, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException("O nome deve possuir ao menos 2 caracteres.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("E-mail inválido.");
        return new User(Guid.NewGuid(), name.Trim(), email.Trim().ToLowerInvariant(), passwordHash, role);
    }

    public void Update(string name, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length < 2)
            throw new DomainException("Nome inválido.");
        Name = name.Trim();
        Role = role;
    }
}
