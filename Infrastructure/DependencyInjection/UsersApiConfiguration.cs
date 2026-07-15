using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UsersAPI.Application.Users;
using UsersAPI.Domain.Users;
using UsersAPI.Infrastructure.Persistence;
using UsersAPI.Infrastructure.Security;

namespace UsersAPI.Infrastructure.DependencyInjection;

public static class UsersApiConfiguration
{
    public static IServiceCollection AddUsersApi(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddUsersApplication();
        services.AddUsersPersistence(configuration);
        services.AddUsersSecurity(configuration);
        services.AddUsersMessaging(configuration);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerWithBearerAuthentication();
        return services;
    }

    public static WebApplication UseUsersApi(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    public static async Task InitializeUsersDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var provider = scope.ServiceProvider;
        await provider.GetRequiredService<UsersDbContext>().Database.EnsureCreatedAsync();

        var repository = provider.GetRequiredService<IUserRepository>();
        var email = app.Configuration["Seed:AdminEmail"] ?? "admin@fcg.com";
        if (await repository.GetByEmailAsync(email, CancellationToken.None) is not null)
            return;

        var password = app.Configuration["Seed:AdminPassword"] ?? "Admin@123";
        var passwordHasher = provider.GetRequiredService<IPasswordHasher>();
        repository.Add(
            User.Create(
                "Administrador",
                email,
                passwordHasher.Hash(password),
                UserRole.Admin
            )
        );
        await repository.SaveChangesAsync(CancellationToken.None);
    }

    private static void AddUsersApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<RegisterUserCommand>()
        );
    }

    private static void AddUsersPersistence(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Database"))
        );
        services.AddScoped<IUserRepository, UserRepository>();
    }

    private static void AddUsersSecurity(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwt = configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        services.AddSingleton(jwt);
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, TokenService>();
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                }
            );
        services.AddAuthorization();
    }

    private static void AddUsersMessaging(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddMassTransit(configurator =>
            configurator.UsingRabbitMq(
                (_, rabbit) =>
                    rabbit.Host(
                        configuration["RabbitMq:Host"] ?? "localhost",
                        "/",
                        host =>
                        {
                            host.Username(configuration["RabbitMq:Username"] ?? "guest");
                            host.Password(configuration["RabbitMq:Password"] ?? "guest");
                        }
                    )
            )
        );
    }

    private static void AddSwaggerWithBearerAuthentication(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe somente o token JWT. O prefixo Bearer será adicionado automaticamente.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            };

            options.AddSecurityDefinition("Bearer", bearerScheme);
            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement { [bearerScheme] = Array.Empty<string>() }
            );
        });
    }
}
