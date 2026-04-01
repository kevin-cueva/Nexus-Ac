using Microsoft.Extensions.DependencyInjection;
using Nexus_api.Infrastructure.Data;
using Nexus_api.Infrastructure.Repository;

namespace Nexus_api.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlServerInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<INexusDbContextFactory<NexusDbContext>>(
            new NexusDbContextFactory(connectionString));
        services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));  //Usado para heredar operaciones hacia tablas en cualquier modelo  

        return services;
    }
}
