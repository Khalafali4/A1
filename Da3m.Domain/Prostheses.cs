using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Da3m.Domain
{
    public partial class Prostheses
    {
        [Key]
        public int DeviceId { get; set; }

        [MaxLength(50)]
        public string LimbType { get; set; } = null!;

        [Column("Length_cm", TypeName = "decimal(6, 2)")]
        public decimal LengthCm { get; set; }

        [Column("Width_cm", TypeName = "decimal(6, 2)")]
        public decimal WidthCm { get; set; }

        [MaxLength(50)]
        public string? Material { get; set; }

        [MaxLength(50)]
        public string? Condition { get; set; }

        public bool IsDeletd { get; set; } = false;

        public bool IsAvailable { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime AddedAt { get; set; }

        [MaxLength(50)]
        public string? Direction { get; set; }

        public int UserId { get; set; }

        [MaxLength(200)]
        public string? Note { get; set; }

        [InverseProperty("Device")]
        public ICollection<Match> Matches { get; set; } = new List<Match>();

        [ForeignKey("UserId")]
        [InverseProperty("Prostheses")]
        public User User { get; set; } = null!;
    }
}