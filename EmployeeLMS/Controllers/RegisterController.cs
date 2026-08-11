using EmployeeLMS.DTO;
using EmployeeLMS.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLMS.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly IAuthService _authService;

        public RegisterController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegistrationDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (success, errorMessage) = await _authService.RegisterAsync(dto);

            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
                return View(dto);
            }

            TempData["SuccessMessage"] = "Account created successfully. Please log in.";
            return RedirectToAction("Index", "Login");
        }
    }
}