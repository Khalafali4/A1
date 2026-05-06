using Da3m.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Da3m.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<Center> Centers { get; }
        IGenericRepository<Doctor> Doctors { get; }
        IGenericRepository<Donation> Donations { get; }
        IGenericRepository<DonorDetail> DonorDetails { get; }
        IGenericRepository<ManufacturerDetail> ManufacturerDetails { get; }
        IGenericRepository<Match> Matches { get; }
        IGenericRepository<Measurement> Measurements { get; }
        IGenericRepository<PatientDetail> PatientDetails { get; }
        IGenericRepository<Prostheses> Prostheses { get; }
        IGenericRepository<VisitReport> VisitReports { get; }

        Task<int> SaveChangesAsync();

    }
}
