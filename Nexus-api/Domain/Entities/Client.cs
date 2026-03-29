using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nexus_api.Domain.Entities;
[Table("clients", Schema = "nexus")]
    public class Client
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("webPage")]
        [MaxLength(500)]
        public string? WebPage { get; set; }

        [Column("idSector")]
        public int IdSector { get; set; }

        // Navegación
        [ForeignKey(nameof(IdSector))]
        public virtual Sector Sector { get; set; } = null!;
        
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }