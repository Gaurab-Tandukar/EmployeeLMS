using EmployeeLMS.Data;
using EmployeeLMS.DTO;
using EmployeeLMS.Models;
using EmployeeLMS.Repositories.Interfaces;
using EmployeeLMS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLMS.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly LibraryDbContext _context;
        private readonly PasswordHasher<Employee> _passwordHasher = new();   // CHANGED: hashes Employee now

        public AuthService(
            IEmployeeRepository employeeRepository,
            IGenericRepository<User> userRepository,
            LibraryDbContext context)
        {
            _employeeRepository = employeeRepository;
            _userRepository = userRepository;
            _context = context;
        }

        // ---------- Register ----------
        public async Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegistrationDTO dto)
        {
            if (await _employeeRepository.EmailExistsAsync(dto.Email))
            {
                return (false, "An account with this email already exists.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var employee = new Employee
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber
                };
                employee.HashPassword = _passwordHasher.HashPassword(employee, dto.Password);

                await _employeeRepository.AddAsync(employee);
                await _employeeRepository.SaveChangesAsync();

                await transaction.CommitAsync();   // ADDED — this was missing
                return (true, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Registration failed. Please try again.");
            }
        }

        // ---------- Login ----------
        public async Task<(User? User, string? ErrorMessage)> LoginAsync(string email, string password)
        {
            var employee = await _employeeRepository.GetByEmailAsync(email);

            if (employee == null)
            {
                return (null, "Invalid email or password.");
            }

            var result = _passwordHasher.VerifyHashedPassword(employee, employee.HashPassword, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return (null, "Invalid email or password.");
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                employee.HashPassword = _passwordHasher.HashPassword(employee, password);
                _employeeRepository.Update(employee);
                await _employeeRepository.SaveChangesAsync();
            }

            if (employee.User == null)
            {
                return (null, "Your account has not been granted access yet. Please contact an administrator.");
            }

            return (employee.User, null);
        }

        // ---------- Email check ----------
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _employeeRepository.EmailExistsAsync(email);
        }

        // ---------- Change password ----------
        // CHANGED: takes staffId now, since the password lives on Employee
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(
            int staffId, string currentPassword, string newPassword)
        {
            var employee = await _employeeRepository.GetByIdAsync(staffId);

            if (employee == null)
            {
                return (false, "Employee not found.");
            }

            var result = _passwordHasher.VerifyHashedPassword(employee, employee.HashPassword, currentPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Current password is incorrect.");
            }

            employee.HashPassword = _passwordHasher.HashPassword(employee, newPassword);
            _employeeRepository.Update(employee);
            await _employeeRepository.SaveChangesAsync();

            return (true, null);
        }

        // ---------- Lookups ----------
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetByIdAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var employee = await _employeeRepository.GetByEmailAsync(email);
            return employee?.User;
        }

        // ---------- Assign role (Admin action) ----------
        public async Task<(bool Success, string? ErrorMessage)> AssignRoleAsync(int staffId, string role, string? adminName = null)
        {
            var employee = await _employeeRepository.GetByIdAsync(staffId);

            if (employee == null)
            {
                return (false, "Employee not found.");
            }

            if (role == "Admin" && string.IsNullOrWhiteSpace(adminName))
            {
                return (false, "Admin name is required when assigning the Admin role.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Admins)
                    .FirstOrDefaultAsync(u => u.StaffID == staffId);

                if (user == null)
                {
                    user = new User
                    {
                        StaffID = staffId,
                        UserRole = role
                    };
                    await _userRepository.AddAsync(user);
                    await _userRepository.SaveChangesAsync(); // flush so UserID is populated
                }
                else
                {
                    user.UserRole = role;
                    _userRepository.Update(user);
                    await _userRepository.SaveChangesAsync();
                }

                // If promoting to Admin, also create the Admin row (if it doesn't already exist)
                if (role == "Admin" && !user.Admins.Any())
                {
                    _context.Admins.Add(new Admin
                    {
                        UserID = user.UserID,
                        Name = adminName!
                    });
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Role assignment failed. Please try again.");
            }
        }

        // ---------- Revoke access (Admin action) ----------
        public async Task<(bool Success, string? ErrorMessage)> RevokeAccessAsync(int staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .Include(u => u.Admins)
                    .FirstOrDefaultAsync(u => u.StaffID == staffId);

                if (user == null)
                {
                    return (true, null); // already not a user — nothing to do
                }

                if (user.Admins.Any())
                {
                    _context.Admins.RemoveRange(user.Admins);
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Failed to revoke access. Please try again.");
            }

        }

        // ---------- Check current access (used by cookie validation) ----------
        public async Task<bool> HasAccessAsync(int staffId)
        {
            return await _context.Users.AnyAsync(u => u.StaffID == staffId);
        }
    }
}