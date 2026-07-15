using MassTransit;
using MediatR;
using UsersAPI.Contracts;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Application.Users;

public class LoginCommand : IRequest<LoginResponseDto?>
{
    public LoginDto Data { get; set; } = new();
}
