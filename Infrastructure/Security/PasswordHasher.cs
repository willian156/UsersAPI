using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using UsersAPI.Domain.Users;

namespace UsersAPI.Infrastructure.Security;

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
        return CryptographicOperations.FixedTimeEquals(expected, Rfc2898DeriveBytes.Pbkdf2(password, Convert.FromBase64String(p[0]), 100_000, HashAlgorithmName.SHA256, 32));
    }
}
