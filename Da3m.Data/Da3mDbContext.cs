using Da3m.Domain;
using Microsoft.EntityFrameworkCore;

namespace Da3m.Data
{

    public partial class Da3mDbContext : DbContext
    {
        public Da3mDbContext()
        {
        }

        public Da3mDbContext(DbContextOptions<Da3mDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Center> Centers { get; set; }

        public virtual DbSet<Doctor> Doctors { get; set; }

        public virtual DbSet<Donation> Donations { get; set; }

        public virtual DbSet<DonorDetail> DonorDetails { get; set; }

        public virtual DbSet<ManufacturerDetail> ManufacturerDetails { get; set; }

        public virtual DbSet<Match> Matches { get; set; }

        public virtual DbSet<Measurement> Measurements { get; set; }

        public virtual DbSet<PatientDetail> PatientDetails { get; set; }

        public virtual DbSet<Prostheses> Prostheses { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<VisitReport> VisitReports { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Data Source=KHALAF;Initial Catalog=Da3m_Db_26;Integrated Security=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Center>(entity =>
            {
                entity.HasKey(e => e.CenterId).HasName("PK__Center__398FC7F720BEADF2");

                entity.Property(e => e.IsActive).HasDefaultValue(true);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Doctor__1788CC4C0D2FE729");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.HasOne(d => d.Center).WithMany(p => p.Doctors).HasConstraintName("FK_Doctor_Center");

                entity.HasOne(d => d.User).WithOne(p => p.Doctor)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Doctor_User");
            });

            modelBuilder.Entity<Donation>(entity =>
            {
                entity.HasKey(e => e.DonationId).HasName("PK__Donation__C5082EFBCF8F1113");

                entity.Property(e => e.DonationDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User).WithMany(p => p.Donations)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Donation_User");
            });

            modelBuilder.Entity<DonorDetail>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__DonorDet__1788CC4C2B7CD459");

                entity.Property(e => e.UserId).ValueGeneratedNever();
                entity.Property(e => e.DonatedDevicesCount).HasDefaultValue(0);
                entity.Property(e => e.TotalDonatedAmount).HasDefaultValue(0m);

                entity.HasOne(d => d.User).WithOne(p => p.DonorDetail)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Donor_User");
            });

            modelBuilder.Entity<ManufacturerDetail>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Manufact__1788CC4CAE650A16");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.HasOne(d => d.User).WithOne(p => p.ManufacturerDetail)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Manufacturer_User");
            });

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(e => e.MatchId).HasName("PK__Matches__4218C817991988DB");

                entity.Property(e => e.MatchDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Device).WithMany(p => p.Matches)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Matches_Device");

                entity.HasOne(d => d.User).WithMany(p => p.Matches)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Matches_User");
            });

            modelBuilder.Entity<Measurement>(entity =>
            {
                entity.HasKey(e => e.MeasurementId).HasName("PK__Measurem__85599FB8FDA0DA98");

                entity.Property(e => e.MeasuredAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User).WithMany(p => p.Measurements)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Measurement_User");
            });

            modelBuilder.Entity<PatientDetail>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__PatientD__1788CC4CC395ED83");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.HasOne(d => d.User).WithOne(p => p.PatientDetail)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Patient_User");
            });

            modelBuilder.Entity<Prostheses>(entity =>
            {
                entity.HasKey(e => e.DeviceId).HasName("PK__Prosthes__49E12311B1B63DBA");

                entity.Property(e => e.AddedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);

                entity.HasOne(d => d.User).WithMany(p => p.Prostheses)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Prostheses_User");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1AAC0A9BD0");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C2B1B7071");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Role");
            });

            modelBuilder.Entity<VisitReport>(entity =>
            {
                entity.HasKey(e => e.ReportId).HasName("PK__VisitRep__D5BD480510351CF1");

                entity.Property(e => e.ReportDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Center).WithMany(p => p.VisitReports)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Report_Center");

                entity.HasOne(d => d.Match).WithMany(p => p.VisitReports)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Report_Match");
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
