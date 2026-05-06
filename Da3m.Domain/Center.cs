using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Da3m.Domain
{

    [Table("Center")]
    public partial class Center
    {
        [Key]
        public int CenterId { get; set; }
        [Required]
        [MaxLength(100)]
        public string CenterName { get; set; } = null!;
        [Required]
        [MaxLength(100)]
        public string LocationText { get; set; } = null!;
        [Required]
        [MaxLength(20)]
        public string Phone { get; set; } = null!;

        public bool IsActive { get; set; } // مغلق مؤققتا ...
        public bool IsDeleted { get; set; }=false;//محذوف  نهائيا  خارج الخدمة

        [InverseProperty("Center")]
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();

        [InverseProperty("Center")]
        public ICollection<VisitReport> VisitReports { get; set; } = new List<VisitReport>();
    }

}
