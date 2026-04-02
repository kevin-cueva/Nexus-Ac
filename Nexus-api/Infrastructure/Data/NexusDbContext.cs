using Microsoft.EntityFrameworkCore;
using Nexus_api.Domain.Entities;

namespace Nexus_api.Infrastructure.Data;

public class NexusDbContext(DbContextOptions<NexusDbContext> options) : DbContext(options)
{
    public DbSet<Case> Cases { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RolTeam> RolTeams { get; set; }
    // No exponemos TeamMember como DbSet público usualmente si es solo tabla intermedia, 
    // pero lo dejamos si necesitas consultar la asignación directamente.
    public DbSet<TeamMember> TeamMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Forzar esquema 'nexus' globalmente 
        modelBuilder.HasDefaultSchema("nexus");

        // Configuración de la tabla intermedia 'Team' con Clave Primaria Compuesta
        modelBuilder.Entity<TeamMember>(entity =>
        {
            entity.HasKey(e => new { e.IdCase, e.IdUser }); // PK Compuesta lógica

            entity.HasIndex(e => e.IdRolTeam); // Índice para rendimiento en búsquedas por rol

            entity.HasOne(e => e.Case)
                  .WithMany(c => c.TeamMembers)
                  .HasForeignKey(e => e.IdCase)
                  .OnDelete(DeleteBehavior.Cascade); // Ajustar política de borrado según negocio

            entity.HasOne(e => e.User)
                  .WithMany(u => u.TeamMemberships)
                  .HasForeignKey(e => e.IdUser)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                  .WithMany(r => r.TeamMembers)
                  .HasForeignKey(e => e.IdRolTeam)
                  .OnDelete(DeleteBehavior.Restrict); // No borrar roles si hay miembros
        });

        // Corrección de la relación Clients -> Sectors
        // En tu DDL original había una confusión, aquí la aseguramos correctamente
        modelBuilder.Entity<Client>()
            .HasOne(c => c.Sector)
            .WithMany(s => s.Clients)
            .HasForeignKey(c => c.IdSector)
            .OnDelete(DeleteBehavior.Restrict);

        // Optimización: Si 'Year' en la BD es solo un entero (ej: 2023) pero mapped como DateTime
        // Podrías necesitar un ValueConverter. Si es TIMESTAMP real, déjalo así.
        // Si prefieres minimalismo de datos, cambia la propiedad en la clase a 'int' y descomenta esto:
        /*
        modelBuilder.Entity<Case>()
            .Property(c => c.Year)
            .HasColumnType("int"); // O el tipo exacto de tu BD
        */
    }
}
