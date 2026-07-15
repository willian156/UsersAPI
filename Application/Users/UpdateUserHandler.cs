using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class UpdateUserHandler(IUserRepository repository) : IRequestHandler<UpdateUserCommand, UserDto?>
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
