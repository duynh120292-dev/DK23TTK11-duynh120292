using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;

namespace PetShopWebsite.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get featured pets
            var featuredPets = await _context.Pets
                .Include(p => p.Category)
                .Where(p => p.IsFeatured && p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            // Get categories
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            // Get latest pets
            var latestPets = await _context.Pets
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedPets = featuredPets;
            ViewBag.Categories = categories;
            ViewBag.LatestPets = latestPets;

            _logger.LogInformation($"Featured pets count: {featuredPets.Count}");
            _logger.LogInformation($"Categories count: {categories.Count}");
            _logger.LogInformation($"Latest pets count: {latestPets.Count}");

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page data");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public async Task<IActionResult> Test()
    {
        var petsCount = await _context.Pets.CountAsync();
        var categoriesCount = await _context.Categories.CountAsync();

        ViewBag.Message = $"Database test: {petsCount} pets, {categoriesCount} categories";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
