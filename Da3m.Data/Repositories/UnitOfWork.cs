using Da3m.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Da3m.Data.Repositories
{

 public class UnitOfWork : IUnitOfWork
    {
        private readonly Da3mDbContext _context;
        private bool _disposed;

        public IGenericRepository<User> Users { get; set; }

        public IGenericRepository<Role> Roles { get; set; }

        public IGenericRepository<Center> Centers { get; set; }

        public IGenericRepository<Doctor> Doctors { get; set; }

        public IGenericRepository<Donation> Donations { get; set; }

        public IGenericRepository<DonorDetail> DonorDetails { get; set; }

        public IGenericRepository<ManufacturerDetail> ManufacturerDetails { get; set; }

        public IGenericRepository<Match> Matches { get; set; }

        public IGenericRepository<Measurement> Measurements { get; set; }

        public IGenericRepository<PatientDetail> PatientDetails { get; set; }

        public IGenericRepository<Prostheses> Prostheses { get; set; }

        public IGenericRepository<VisitReport> VisitReports { get; set; }

        public UnitOfWork(Da3mDbContext context)
        {
            _context = context;
            Users = new GenericRepository<User>(context);
            Roles = new GenericRepository<Role>(context);
            Centers = new GenericRepository<Center>(context);
            Doctors = new GenericRepository<Doctor>(context);
            Donations = new GenericRepository<Donation>(context);
            DonorDetails = new GenericRepository<DonorDetail>(context);
            ManufacturerDetails = new GenericRepository<ManufacturerDetail>(context);
            Matches = new GenericRepository<Match>(context);
            Measurements = new GenericRepository<Measurement>(context);
            PatientDetails = new GenericRepository<PatientDetail>(context);
            Prostheses = new GenericRepository<Prostheses>(context);
            VisitReports = new GenericRepository<VisitReport>(context);

        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _context.Dispose();

                _disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
