using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;
using Microsoft.AspNetCore.Authorization;

namespace PetShopWebsite.Controllers;

public class PetsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PetsController> _logger;

    public PetsController(ApplicationDbContext context, ILogger<PetsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Pets
    public async Task<IActionResult> Index(int? categoryId, string? search, decimal? minPrice, decimal? maxPrice, string? sortBy, int page = 1)
    {
        const int pageSize = 12;
        
        var query = _context.Pets
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        // Search by name or breed
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Breed.Contains(search));
        }

        // Filter by price range
        if (minPrice.HasValue)
        {
            query = query.Where(p => (p.SalePrice ?? p.Price) >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => (p.SalePrice ?? p.Price) <= maxPrice.Value);
        }

        // Sort
        query = sortBy switch
        {
            "price_asc" => query.OrderBy(p => p.SalePrice ?? p.Price),
            "price_desc" => query.OrderByDescending(p => p.SalePrice ?? p.Price),
            "name" => query.OrderBy(p => p.Name),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt)
        };

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get categories for filter
        var categories = await _context.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.CurrentCategoryId = categoryId;
        ViewBag.CurrentSearch = search;
        ViewBag.CurrentMinPrice = minPrice;
        ViewBag.CurrentMaxPrice = maxPrice;
        ViewBag.CurrentSortBy = sortBy;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(pets);
    }

    // GET: Pets/Featured
    public async Task<IActionResult> Featured()
    {
        var featuredPets = await _context.Pets
            .Include(p => p.Category)
            .Where(p => p.IsFeatured && p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return View(featuredPets);
    }

    // GET: Pets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pet = await _context.Pets
            .Include(p => p.Category)
            .Include(p => p.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

        if (pet == null)
        {
            return NotFound();
        }

        // Get related pets (same category)
        var relatedPets = await _context.Pets
            .Include(p => p.Category)
            .Where(p => p.CategoryId == pet.CategoryId && p.Id != pet.Id && p.IsActive)
            .Take(4)
            .ToListAsync();

        ViewBag.RelatedPets = relatedPets;

        return View(pet);
    }

    // GET: Pets/Category/1
    public async Task<IActionResult> Category(int id, int page = 1)
    {
        const int pageSize = 12;

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null)
        {
            return NotFound();
        }

        var query = _context.Pets
            .Include(p => p.Category)
            .Where(p => p.CategoryId == id && p.IsActive)
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.Category = category;
        ViewBag.CategoryName = category.Name;
        ViewBag.CategoryDescription = category.Description;
        ViewBag.CategoryId = category.Id;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View(pets);
    }

    // GET: Pets/Search
    public async Task<IActionResult> Search(string q, int page = 1)
    {
        const int pageSize = 12;

        if (string.IsNullOrEmpty(q))
        {
            return RedirectToAction(nameof(Index));
        }

        var query = _context.Pets
            .Include(p => p.Category)
            .Where(p => p.IsActive && (p.Name.Contains(q) || p.Breed.Contains(q) || p.Description!.Contains(q)))
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.CreatedAt);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var pets = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.SearchQuery = q;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalItems = totalItems;

        return View("Index", pets);
    }

    // POST: Pets/AddToCart - Redirect to Cart controller
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int petId, int quantity = 1)
    {
        return RedirectToAction("AddToCart", "Cart", new { petId = petId, quantity = quantity });
    }
}
