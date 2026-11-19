using Microsoft.EntityFrameworkCore;

namespace UsersAPI.Infrastructure.DependencyInjection;

internal static class DatabaseInitialization
{
    public static async Task RunAsync(DbContext database, Func<Task> initialize)
    {
        // The database is provisioned by infrastructure; serialize schema/seed setup across replicas.
        await database.Database.OpenConnectionAsync();
        try
        {
            await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_lock(748390)");
            await initialize();
        }
        finally
        {
            await database.Database.ExecuteSqlRawAsync("SELECT pg_advisory_unlock(748390)");
            await database.Database.CloseConnectionAsync();
        }
    }
}
