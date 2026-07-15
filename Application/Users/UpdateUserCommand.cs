using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class UpdateUserCommand : IRequest<UserDto?>
{
    public Guid Id { get; set; }
    public UpdateUserDto Data { get; set; } = new();
}
