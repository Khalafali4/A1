using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Da3m.Domain
{
    public partial class VisitReport
    {
        [Key]
        public int ReportId { get; set; }

        public int MatchId { get; set; }

        public int CenterId { get; set; }

        [MaxLength(200)]
        public string? DoctorNotes { get; set; }

        [MaxLength(200)]
        public string? PatientFeedback { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime ReportDate { get; set; }

        [ForeignKey("CenterId")]
        [InverseProperty("VisitReports")]
        public Center Center { get; set; } = null!;

        [ForeignKey("MatchId")]
        [InverseProperty("VisitReports")]
        public Match Match { get; set; } = null!;
    }

}