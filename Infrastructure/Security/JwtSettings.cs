using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Security;

public class JwtSettings
{
    public string Issuer { get; set; } = "FCG";
    public string Audience { get; set; } = "FCG";
    public string Key { get; set; } = "change-this-development-key-with-32-chars";
    public int ExpiresMinutes { get; set; } = 120;
}
