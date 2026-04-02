using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nexus_api.Domain.Entities;
[Table("RolTeam", Schema = "nexus")] 
    public class RolTeam
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        [MaxLength(500)]
        public string? Description { get; set; }

        // Navegación
        public virtual ICollection<TeamMember> TeamMembers { get; set; } = [];
    }