using EmployeeLMS.Models;
using EmployeeLMS.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

public class CategoryController : Controller
{
    private readonly IGenericRepository<Category> _categoryRepo;

    public CategoryController(IGenericRepository<Category> categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    // GET: /Category
    public async Task<IActionResult> Index()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return View(categories);
    }

    // GET: /Category/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
        if (ModelState.IsValid)
        {
            await _categoryRepo.AddAsync(category);
            await _categoryRepo.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(category);
    }
}