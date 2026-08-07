using EmployeeLMS.Data;
using EmployeeLMS.DTO;
using EmployeeLMS.Models;
using EmployeeLMS.Repositories.Interfaces;
using EmployeeLMS.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace EmployeeLMS.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly LibraryDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

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

                await _employeeRepository.AddAsync(employee);
                await _employeeRepository.SaveChangesAsync(); // flush so employee.StaffID is populated

                var user = new User
                {
                    StaffID = employee.StaffID,
                    UserRole = "Staff"
                };
                user.HashPassword = _passwordHasher.HashPassword(user, dto.Password);

                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, null);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return (false, "Registration failed. Please try again.");
            }
        }

        // ---------- Login ----------
        public async Task<User?> LoginAsync(string email, string password)
        {
            var employee = await _employeeRepository.GetByEmailAsync(email);

            if (employee?.User == null)
            {
                return null; // no account with this email, or employee has no linked User
            }

            var result = _passwordHasher.VerifyHashedPassword(
                employee.User, employee.User.HashPassword, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // If the hasher flags that the stored hash uses outdated parameters, re-hash and save.
            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                employee.User.HashPassword = _passwordHasher.HashPassword(employee.User, password);
                _userRepository.Update(employee.User);
                await _userRepository.SaveChangesAsync();
            }

            return employee.User;
        }

        // ---------- Email check ----------
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _employeeRepository.EmailExistsAsync(email);
        }

        // ---------- Change password ----------
        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(
            int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return (false, "User not found.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.HashPassword, currentPassword);

            if (result == PasswordVerificationResult.Failed)
            {
                return (false, "Current password is incorrect.");
            }

            user.HashPassword = _passwordHasher.HashPassword(user, newPassword);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

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
    }
}