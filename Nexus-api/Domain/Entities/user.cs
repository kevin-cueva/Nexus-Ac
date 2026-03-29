using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Nexus_api.Domain.Entities;
[Table("users", Schema = "nexus")]
    public class User
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        // Navegación inversa (Muchos a Muchos explícito vía Team)
        public virtual ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();
    }