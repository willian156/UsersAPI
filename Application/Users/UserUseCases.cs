using MassTransit;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class RegisterUserCommand : IRequest<UserDto>
{
    public RegisterUserDto Data { get; set; } = new();
}

public class LoginCommand : IRequest<LoginResponseDto?>
{
    public LoginDto Data { get; set; } = new();
}

public class UpdateUserCommand : IRequest<UserDto?>
{
    public Guid Id { get; set; }
    public UpdateUserDto Data { get; set; } = new();
}

public class DeleteUserCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; set; }
}

public class GetUsersQuery : IRequest<IReadOnlyList<UserDto>> { }

public static class UserMapping
{
    public static UserDto ToDto(User u) =>
        new()
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            CreatedAt = u.CreatedAt,
        };
}

public class RegisterUserHandler(
    IUserRepository repository,
    IPasswordHasher hasher,
    IPublishEndpoint publisher
) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand c, CancellationToken ct)
    {
        if (c.Data.Password.Length < 8)
            throw new DomainException("A senha deve possuir ao menos 8 caracteres.");
        if (
            await repository.GetByEmailAsync(c.Data.Email.Trim().ToLowerInvariant(), ct) is not null
        )
            throw new InvalidOperationException("E-mail já cadastrado.");
        var user = User.Create(c.Data.Name, c.Data.Email, hasher.Hash(c.Data.Password));
        repository.Add(user);
        await repository.SaveChangesAsync(ct);
        await publisher.Publish(
            new UserCreatedEvent
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
            },
            ct
        );
        return UserMapping.ToDto(user);
    }
}

public class LoginHandler(IUserRepository repository, IPasswordHasher hasher, ITokenService tokens)
    : IRequestHandler<LoginCommand, LoginResponseDto?>
{
    public async Task<LoginResponseDto?> Handle(LoginCommand c, CancellationToken ct)
    {
        var user = await repository.GetByEmailAsync(c.Data.Email.Trim().ToLowerInvariant(), ct);
        return user is null || !hasher.Verify(c.Data.Password, user.PasswordHash)
            ? null
            : new LoginResponseDto { Token = tokens.Create(user), User = UserMapping.ToDto(user) };
    }
}

public class UpdateUserHandler(IUserRepository repository)
    : IRequestHandler<UpdateUserCommand, UserDto?>
{
    public async Task<UserDto?> Handle(UpdateUserCommand c, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(c.Id, ct);
        if (user is null)
            return null;
        if (!Enum.TryParse<UserRole>(c.Data.Role, true, out var role))
            throw new DomainException("Perfil inválido.");
        user.Update(c.Data.Name, role);
        await repository.SaveChangesAsync(ct);
        return UserMapping.ToDto(user);
    }
}

public class DeleteUserHandler(IUserRepository repository)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand c, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(c.Id, ct);
        if (user is null)
            return false;
        repository.Remove(user);
        await repository.SaveChangesAsync(ct);
        return true;
    }
}

public class GetUserByIdHandler(IUserRepository repository)
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery q, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(q.Id, ct);
        return user is null ? null : UserMapping.ToDto(user);
    }
}

public class GetUsersHandler(IUserRepository repository)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery q, CancellationToken ct) =>
        (await repository.GetAllAsync(ct)).Select(UserMapping.ToDto).ToList();
}
