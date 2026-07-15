using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class GetUserByIdHandler(IUserRepository repository) : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery q, CancellationToken ct)
    {
        var user = await repository.GetByIdAsync(q.Id, ct);
        return user is null ? null : UserMapping.ToDto(user);
    }
}
