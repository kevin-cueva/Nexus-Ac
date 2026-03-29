using Microsoft.EntityFrameworkCore;

namespace Nexus_api.Infrastructure.Data;

public class NexusDbContextFactory(string connectionString) : INexusDbContextFactory<NexusDbContext>
{
    private readonly string _connectionString = connectionString;

    public NexusDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<NexusDbContext>();
        optionsBuilder.UseSqlServer(
            _connectionString,
            sqlServerOptions =>
            {
                sqlServerOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

        return new NexusDbContext(optionsBuilder.Options);
    }
}
