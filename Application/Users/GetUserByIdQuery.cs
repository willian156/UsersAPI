using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class GetUserByIdQuery : IRequest<UserDto?>
{
    public Guid Id { get; set; }
}
