using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Da3m.Domain
{

    [Table("Doctor")]
    public partial class Doctor
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(100)]
        public string? Specialty { get; set; }

        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        [MaxLength(100)]
        public string? HospitalName { get; set; }
        public bool IsDeletd { get; set; } = false;

        public int? CenterId { get; set; }

        public Center? Center { get; set; }

        public User User { get; set; } = null!;
    }

}