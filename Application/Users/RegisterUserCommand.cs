using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class RegisterUserCommand : IRequest<UserDto>
{
    public RegisterUserDto Data { get; set; } = new();
}
