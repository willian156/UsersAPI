using UsersAPI.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUsersApi(builder.Configuration);

var app = builder.Build();

await app.InitializeUsersDatabaseAsync();
app.UseUsersApi();

app.Run();
