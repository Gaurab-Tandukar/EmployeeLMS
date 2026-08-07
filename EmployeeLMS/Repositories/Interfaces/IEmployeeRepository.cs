using EmployeeLMS.Models;

namespace EmployeeLMS.Repositories.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        Task<Employee?> GetByEmailAsync(string email);
        Task<bool> EmailExistsAsync(string email);
    }
}