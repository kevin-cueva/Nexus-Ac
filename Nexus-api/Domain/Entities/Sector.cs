// Esquema común para todas las tablas
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus_api.Domain.Entities;
[Table("sectors", Schema = "nexus")]
    public class Sector
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(255)] // Ajustar según necesidad real de la BD
        public string Name { get; set; } = string.Empty;

        // Navegación
        public virtual ICollection<Client> Clients { get; set; } = new List<Client>();
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }