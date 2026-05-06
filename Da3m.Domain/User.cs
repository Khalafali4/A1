using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Da3m.Domain
{
    [Table("User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;
        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime CreatedAt { get; set; }

        public int RoleId { get; set; }

        public bool IsDeleted { get; set; } = false;

        [InverseProperty("User")]
        public Doctor? Doctor { get; set; }

        [InverseProperty("User")]
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();

        [InverseProperty("User")]
        public DonorDetail? DonorDetail { get; set; }

        [InverseProperty("User")]
        public ManufacturerDetail? ManufacturerDetail { get; set; }

        [InverseProperty("User")]
        public ICollection<Match> Matches { get; set; } = new List<Match>();

        [InverseProperty("User")]
        public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();

        [InverseProperty("User")]
        public PatientDetail? PatientDetail { get; set; }

        [InverseProperty("User")]
        public ICollection<Prostheses> Prostheses { get; set; } = new List<Prostheses>();

        [ForeignKey("RoleId")]
        [InverseProperty("Users")]
        public Role Role { get; set; } = null!;
    }

}