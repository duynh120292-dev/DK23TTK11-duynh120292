using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;

namespace PetShopWebsite.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Get pet count for each category
            foreach (var category in categories)
            {
                category.PetCount = await _context.Pets
                    .CountAsync(p => p.CategoryId == category.Id && p.IsActive);
            }

            return View(categories);
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

            if (category == null)
            {
                return NotFound();
            }

            // Get pets in this category
            var pets = await _context.Pets
                .Include(p => p.Category)
                .Where(p => p.CategoryId == id && p.IsActive)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(12)
                .ToListAsync();

            ViewBag.Pets = pets;
            ViewBag.PetCount = await _context.Pets
                .CountAsync(p => p.CategoryId == id && p.IsActive);

            return View(category);
        }
    }
}
