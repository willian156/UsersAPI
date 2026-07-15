using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Security;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string stored);
}

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string stored)
    {
        var p = stored.Split('.');
        if (p.Length != 2)
            return false;
        var expected = Convert.FromBase64String(p[1]);
        return CryptographicOperations.FixedTimeEquals(
            expected,
            Rfc2898DeriveBytes.Pbkdf2(
                password,
                Convert.FromBase64String(p[0]),
                100_000,
                HashAlgorithmName.SHA256,
                32
            )
        );
    }
}

public class JwtSettings
{
    public string Issuer { get; set; } = "FCG";
    public string Audience { get; set; } = "FCG";
    public string Key { get; set; } = "change-this-development-key-with-32-chars";
    public int ExpiresMinutes { get; set; } = 120;
}

public interface ITokenService
{
    string Create(User user);
}

public class TokenService(JwtSettings settings) : ITokenService
{
    public string Create(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
        };
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiresMinutes),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
                SecurityAlgorithms.HmacSha256
            )
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
