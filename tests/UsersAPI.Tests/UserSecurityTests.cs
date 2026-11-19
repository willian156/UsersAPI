using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Security;
using Xunit;

namespace UsersAPI.Tests;

public sealed class UserSecurityTests
{
    [Fact]
    public void PasswordHashUsesRandomSaltAndRejectsWrongPassword()
    {
        var hasher = new PasswordHasher();
        var password = Guid.NewGuid().ToString();
        var hash = hasher.Hash(password);
        Assert.NotEqual(hash, hasher.Hash(password));
        Assert.True(hasher.Verify(password, hash));
        Assert.False(hasher.Verify("incorrect", hash));
    }

    [Fact]
    public void NewUserIsNotAnAdminAndEmailIsNormalized()
    {
        var user = User.Create(" Jogador ", " PLAYER@example.com ", "hash");
        Assert.Equal(UserRole.User, user.Role);
        Assert.Equal("player@example.com", user.Email);
        Assert.Equal("Jogador", user.Name);
    }

    [Theory]
    [InlineData("", "player@example.com")]
    [InlineData("Jogador", "invalid")]
    public void InvalidUserIsRejected(string name, string email) =>
        Assert.Throws<DomainException>(() => User.Create(name, email, "hash"));
}
