using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class GetUsersHandler(IUserRepository repository) : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery q, CancellationToken ct) => (await repository.GetAllAsync(ct)).Select(UserMapping.ToDto).ToList();
}
