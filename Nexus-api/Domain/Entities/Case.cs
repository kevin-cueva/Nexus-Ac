using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexus_api.Domain.Entities;
  [Table("cases", Schema = "nexus")]
    public class Case
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [Column("idClient")]
        public int IdClient { get; set; }

        [Column("country")]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        [Column("year")] 
        public DateTime Year { get; set; }

        [Column("duration")]
        [MaxLength(50)]
        public string Duration { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(2000)] // Texto largo
        public string? Description { get; set; }

        [Column("idSector")]
        public int IdSector { get; set; }

        // Navegaciones
        [ForeignKey(nameof(IdClient))]
        public virtual Client Client { get; set; } = null!;

        [ForeignKey(nameof(IdSector))]
        public virtual Sector Sector { get; set; } = null!;

        public virtual ICollection<TeamMember> TeamMembers { get; set; } = [];
    }