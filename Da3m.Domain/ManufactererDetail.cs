using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{
    public partial class ManufacturerDetail
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } = null!;
        public bool IsDeletd { get; set; } = false;

        [MaxLength(50)]
        public string? CommercialRegister { get; set; }

        [MaxLength(100)]
        public string? Website { get; set; }

        [ForeignKey("UserId")]
        [InverseProperty("ManufacturerDetail")]
        public virtual User User { get; set; } = null!;
    }

}
