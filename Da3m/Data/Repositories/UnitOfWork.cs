using Da3m.Domain;
namespace Da3m.Data.Repositories
{
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly Da3mDbContext _context;
        private bool _disposed;

        // lazy backing fields to avoid heavy startup allocations
        private IGenericRepository<User>? _users;
        private IGenericRepository<Role>? _roles;
        private IGenericRepository<Center>? _centers;
        private IGenericRepository<Doctor>? _doctors;
        private IGenericRepository<Donation>? _donations;
        private IGenericRepository<DonorDetail>? _donorDetails;
        private IGenericRepository<ManufacturerDetail>? _manufacturerDetails;
        private IGenericRepository<Match>? _matches;
        private IGenericRepository<Measurement>? _measurements;
        private IGenericRepository<PatientDetail>? _patientDetails;
        private IGenericRepository<Prostheses>? _prostheses;
        private IGenericRepository<VisitReport>? _visitReports;

        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IGenericRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);
        public IGenericRepository<Center> Centers => _centers ??= new GenericRepository<Center>(_context);
        public IGenericRepository<Doctor> Doctors => _doctors ??= new GenericRepository<Doctor>(_context);
        public IGenericRepository<Donation> Donations => _donations ??= new GenericRepository<Donation>(_context);
        public IGenericRepository<DonorDetail> DonorDetails => _donorDetails ??= new GenericRepository<DonorDetail>(_context);
        public IGenericRepository<ManufacturerDetail> ManufacturerDetails => _manufacturerDetails ??= new GenericRepository<ManufacturerDetail>(_context);
        public IGenericRepository<Match> Matches => _matches ??= new GenericRepository<Match>(_context);
        public IGenericRepository<Measurement> Measurements => _measurements ??= new GenericRepository<Measurement>(_context);
        public IGenericRepository<PatientDetail> PatientDetails => _patientDetails ??= new GenericRepository<PatientDetail>(_context);
        public IGenericRepository<Prostheses> Prostheses => _prostheses ??= new GenericRepository<Prostheses>(_context);
        public IGenericRepository<VisitReport> VisitReports => _visitReports ??= new GenericRepository<VisitReport>(_context);

        public UnitOfWork(Da3mDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Standard dispose pattern
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // dispose context and clear references to repositories
                    _context.Dispose();

                    _users = null;
                    _roles = null;
                    _centers = null;
                    _doctors = null;
                    _donations = null;
                    _donorDetails = null;
                    _manufacturerDetails = null;
                    _matches = null;
                    _measurements = null;
                    _patientDetails = null;
                    _prostheses = null;
                    _visitReports = null;
                }

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
            // forward to DbContext async save
            return await _context.SaveChangesAsync();
        }

      
    }
}