using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class RegisterUserHandler(IUserRepository repository, IPasswordHasher hasher, IPublishEndpoint publisher) : IRequestHandler<RegisterUserCommand, UserDto>
{
    public async Task<UserDto> Handle(RegisterUserCommand c, CancellationToken ct)
    {
        if (c.Data.Password.Length < 8)
            throw new DomainException("A senha deve possuir ao menos 8 caracteres.");
        if (await repository.GetByEmailAsync(c.Data.Email.Trim().ToLowerInvariant(), ct) is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");
        var user = User.Create(c.Data.Name, c.Data.Email, hasher.Hash(c.Data.Password));
        repository.Add(user);
        await repository.SaveChangesAsync(ct);
        await publisher.Publish(new UserCreatedEvent { UserId = user.Id, Name = user.Name, Email = user.Email, CreatedAt = user.CreatedAt, }, ct);
        return UserMapping.ToDto(user);
    }
}
