using EmployeeLMS.Data;
using EmployeeLMS.Models;
using EmployeeLMS.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLMS.Repositories.Implementation
{
    public class EmployeeRepository : GenericRepository<Employee>, IEmployeeRepository
    {
        private readonly LibraryDbContext _context;

        public EmployeeRepository(LibraryDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Employees.AnyAsync(e => e.Email == email);
        }
    }
}