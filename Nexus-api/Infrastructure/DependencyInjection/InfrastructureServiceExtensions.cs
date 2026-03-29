using Microsoft.Extensions.DependencyInjection;
using Nexus_api.Infrastructure.Data;

namespace Nexus_api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<INexusDbContextFactory<NexusDbContext>>(
            new NexusDbContextFactory(connectionString));

        return services;
    }
}
