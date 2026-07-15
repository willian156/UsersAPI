using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Security;

public interface ITokenService
{
    string Create(User user);
}
