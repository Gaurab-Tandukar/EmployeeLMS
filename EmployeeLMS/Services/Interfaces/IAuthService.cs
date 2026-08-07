using EmployeeLMS.DTO;
using EmployeeLMS.Models;

namespace EmployeeLMS.Services.Interfaces
{
    public interface IAuthService
    {
        // Core auth
        Task<(bool Success, string? ErrorMessage)> RegisterAsync(RegistrationDTO dto);
        Task<User?> LoginAsync(string email, string password);

        // Supporting checks
        Task<bool> EmailExistsAsync(string email);

        // Password management
        Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(int userId, string currentPassword, string newPassword);

        // Session/identity helpers
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
    }
}
