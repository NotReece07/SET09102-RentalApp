using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;

namespace StarterApp.Test.Fixtures;

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; }

    public DatabaseFixture()
    {
        ConnectionString =
            Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Host=db;Port=5432;Username=app_user;Password=app_password;Database=appdb";
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString, npgsqlOptions =>
            {
                npgsqlOptions.UseNetTopologySuite(); // enables PostGIS support for repository integration tests
            })
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync(); // makes sure the test database has the latest schema before repository tests run
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}