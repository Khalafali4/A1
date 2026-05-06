using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{

    [Table("Measurement")]
    public partial class Measurement
    {
        [Key]
        public int MeasurementId { get; set; }

        [MaxLength(50)]
        public string LimbType { get; set; } = null!;

        [Column("Length_cm", TypeName = "decimal(6, 2)")]
        public decimal LengthCm { get; set; }

        [Column("Width_cm", TypeName = "decimal(6, 2)")]
        public decimal WidthCm { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime MeasuredAt { get; set; }

        [MaxLength(200)]
        public string? Notes { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Measurements")]
        public User User { get; set; } = null!;
    }

}
