using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{
    public partial class PatientDetail
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string NationalId { get; set; } = null!;

        public DateOnly BirthDate { get; set; }

        [MaxLength(10)]
        public string? Gender { get; set; } = null!;

        [MaxLength(150)]
        public string? Address { get; set; }
        public bool IsDeleted { get; set; } = false;

        [MaxLength(100)]
        public string? DisabilityType { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("PatientDetail")]
        public User User { get; set; } = null!;
    }
}
