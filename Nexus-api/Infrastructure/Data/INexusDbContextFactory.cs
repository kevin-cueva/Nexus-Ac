using Microsoft.EntityFrameworkCore;

namespace Nexus_api.Infrastructure.Data;

public interface INexusDbContextFactory<out TContext> where TContext : DbContext
{
    TContext CreateDbContext();
}
