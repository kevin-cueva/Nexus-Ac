using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus_api.Domain.Entities;
// Tabla intermedia para la relación Many-to-Many entre Case y User
    [Table("Team", Schema = "nexus")]
    public class TeamMember
    {
        // Clave primaria compuesta o ID propio. 
        // Tu DDL no tiene PK explícita en Team, usualmente es la combinación de las FKs.
        // Agregamos un ID para facilitar el manejo en EF, o usamos Keyless si es estricto.
        // Opción recomendada: Clave Primaria Compuesta.
        
        [Column("idCase")]
        public int IdCase { get; set; }

        [Column("idUser")]
        public int IdUser { get; set; }

        [Column("idRolTeam")]
        public int IdRolTeam { get; set; }

        // Definición de PK Compuesta
        [Key]
        [Column("id")]
        public int Id { get; set; } // Truco para EF Core 5+ o configurar en OnModelCreating

        // Navegaciones
        [ForeignKey(nameof(IdCase))]
        public virtual Case Case { get; set; } = null!;

        [ForeignKey(nameof(IdUser))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(IdRolTeam))]
        public virtual RolTeam Role { get; set; } = null!;
    }
    
    // Helper para clave compuesta si no quieres usar Fluent API exclusivamente para la PK
    public class CompositeKey { } 
