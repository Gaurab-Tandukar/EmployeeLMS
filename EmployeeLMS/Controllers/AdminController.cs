using EmployeeLMS.Repositories.Interfaces;
using EmployeeLMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLMS.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IEmployeeRepository _employeeRepository;

        public AdminController(IAuthService authService, IEmployeeRepository employeeRepository)
        {
            _authService = authService;
            _employeeRepository = employeeRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentStaffId = int.Parse(User.FindFirst("StaffID")!.Value);

            var employees = (await _employeeRepository.GetAllWithUserAsync())
                .Where(e => e.StaffID != currentStaffId);

            return View(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAccess(int staffId, bool isUser)
        {
            var currentStaffId = int.Parse(User.FindFirst("StaffID")!.Value);

            if (staffId == currentStaffId)
            {
                TempData["ErrorMessage"] = "You cannot change your own access.";
                return RedirectToAction("Index");
            }

            var (success, errorMessage) = isUser
                ? await _authService.AssignRoleAsync(staffId, "Staff")
                : await _authService.RevokeAccessAsync(staffId);

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Access updated." : errorMessage;

            return RedirectToAction("Index");
        }
    }
}