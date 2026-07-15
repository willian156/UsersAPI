using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class DeleteUserHandler(IUserRepository repository) : IRequestHandler<DeleteUserCommand, bool>
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
