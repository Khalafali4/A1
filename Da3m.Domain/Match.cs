using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Da3m.Domain
{
    public partial class Match
    {
        [Key]
        public int MatchId { get; set; }

        public int UserId { get; set; }
        public bool IsDeleted { get; set; } = false;

        public int DeviceId { get; set; }

        [Range(0, 100)]
        public decimal MatchPercentage { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime MatchDate { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = null!;

        [ForeignKey("DeviceId")]
        [InverseProperty("Matches")]
        public Prostheses Device { get; set; } = null!;

        [ForeignKey("UserId")]
        [InverseProperty("Matches")]
        public User User { get; set; } = null!;

        [InverseProperty("Match")]
        public ICollection<VisitReport> VisitReports { get; set; } = new List<VisitReport>();
    }

}
