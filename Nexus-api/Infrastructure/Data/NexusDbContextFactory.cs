using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Nexus_api.Infrastructure.Data;

// ⚠️ ¡Ningún parámetro en la declaración de la clase!
public class NexusDbContextFactory : IDesignTimeDbContextFactory<NexusDbContext>
{
    public NexusDbContext CreateDbContext(string[] args)
    {
        // 1. Carga appsettings.json manualmente
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables() // opcional: para overrides
            .Build();

        // 2. Extrae la cadena de conexión desde la sección "Configuraciones"
        var connectionString = config["ConnectionStrings:DbNexus"];
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException(
                "La cadena de conexión no se encontró en appsettings.json → Configuraciones:CadenaConexion");

        // 3. Configura el DbContext
        var optionsBuilder = new DbContextOptionsBuilder<NexusDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new NexusDbContext(optionsBuilder.Options);
    }
}
