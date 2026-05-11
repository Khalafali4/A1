using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{

    [Table("Donation")]
    public partial class Donation
    {
        [Key]
        public int DonationId { get; set; }

        public int UserId { get; set; }
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime DonationDate { get; set; }
        public bool IsDeleted { get; set; } = false;

        [MaxLength(200)]
        public string? Note { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("Donations")]
        public User User { get; set; } = null!;
    }

}
