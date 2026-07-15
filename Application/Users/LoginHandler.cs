using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class LoginHandler(IUserRepository repository, IPasswordHasher hasher, ITokenService tokens) : IRequestHandler<LoginCommand, LoginResponseDto?>
{
    public async Task<LoginResponseDto?> Handle(LoginCommand c, CancellationToken ct)
    {
        var user = await repository.GetByEmailAsync(c.Data.Email.Trim().ToLowerInvariant(), ct);
        return user is null || !hasher.Verify(c.Data.Password, user.PasswordHash) ? null : new LoginResponseDto
        {
            Token = tokens.Create(user),
            User = UserMapping.ToDto(user)
        };
    }
}
