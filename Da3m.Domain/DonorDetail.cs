using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{
    public partial class DonorDetail
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [MaxLength(50)]
        public string? PreferredDonationType { get; set; }

        public decimal? TotalDonatedAmount { get; set; }

        public int? DonatedDevicesCount { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("DonorDetail")]
        public User User { get; set; } = null!;
    }

}
