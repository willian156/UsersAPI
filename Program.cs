using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new();
var usersStore = new ConcurrentDictionary<Guid, User>();
var adminEmail = builder.Configuration["Seed:AdminEmail"] ?? "admin@fcg.com";
var adminPassword = builder.Configuration["Seed:AdminPassword"] ?? "Admin@123";
var admin = new User(Guid.NewGuid(), "Administrador", adminEmail.ToLowerInvariant(), Hash(adminPassword), "Admin");
usersStore[admin.Id] = admin;
builder.Services.AddSingleton(usersStore);
builder.Services.AddMassTransit(x => x.UsingRabbitMq((context, cfg) =>
    cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
    {
        h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
        h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
    })));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
    o.TokenValidationParameters = TokenParameters(jwt));
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger(); app.UseSwaggerUI(); app.UseAuthentication(); app.UseAuthorization();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapPost("/users", async (RegisterRequest request, ConcurrentDictionary<Guid, User> users, IPublishEndpoint bus) =>
{
    var email = request.Email.Trim().ToLowerInvariant();
    if (request.Name.Trim().Length < 2 || !email.Contains('@') || request.Password.Length < 8)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["user"] = ["Nome, e-mail ou senha invalidos (senha minima: 8 caracteres)."] });
    if (users.Values.Any(x => x.Email == email)) return Results.Conflict(new { message = "E-mail ja cadastrado." });
    var user = new User(Guid.NewGuid(), request.Name.Trim(), email, Hash(request.Password), "User");
    users[user.Id] = user;
    await bus.Publish(new Fcg.Contracts.UserCreatedEvent(user.Id, user.Name, user.Email, DateTimeOffset.UtcNow));
    return Results.Created($"/users/{user.Id}", new UserResponse(user.Id, user.Name, user.Email, user.Role));
});
app.MapPost("/auth/login", (LoginRequest request, ConcurrentDictionary<Guid, User> users) =>
{
    var user = users.Values.FirstOrDefault(x => x.Email == request.Email.Trim().ToLowerInvariant());
    if (user is null || !Verify(request.Password, user.PasswordHash)) return Results.Unauthorized();
    var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(ClaimTypes.Role, user.Role) };
    var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: DateTime.UtcNow.AddMinutes(jwt.ExpiresMinutes), signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)), SecurityAlgorithms.HmacSha256));
    return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token), user = new UserResponse(user.Id, user.Name, user.Email, user.Role) });
});
app.MapGet("/users/{id:guid}", (Guid id, ConcurrentDictionary<Guid, User> users) => users.TryGetValue(id, out var user) ? Results.Ok(new UserResponse(user.Id, user.Name, user.Email, user.Role)) : Results.NotFound()).RequireAuthorization();
app.Run();

static string Hash(string password) { var salt = RandomNumberGenerator.GetBytes(16); var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32); return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}"; }
static bool Verify(string password, string stored) { var p = stored.Split('.'); if (p.Length != 2) return false; var salt = Convert.FromBase64String(p[0]); var expected = Convert.FromBase64String(p[1]); return CryptographicOperations.FixedTimeEquals(expected, Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32)); }
static TokenValidationParameters TokenParameters(JwtSettings s) => new() { ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidIssuer = s.Issuer, ValidAudience = s.Audience, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.Key)), ClockSkew = TimeSpan.FromSeconds(30) };
record RegisterRequest(string Name, string Email, string Password); record LoginRequest(string Email, string Password); record UserResponse(Guid Id, string Name, string Email, string Role); record User(Guid Id, string Name, string Email, string PasswordHash, string Role);
sealed class JwtSettings { public string Issuer { get; set; } = "FCG"; public string Audience { get; set; } = "FCG"; public string Key { get; set; } = "change-this-development-key-with-32-chars"; public int ExpiresMinutes { get; set; } = 120; }
namespace Fcg.Contracts { public record UserCreatedEvent(Guid UserId, string Name, string Email, DateTimeOffset CreatedAt); }
