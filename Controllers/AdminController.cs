using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;
using PetShopWebsite.ViewModels;

namespace PetShopWebsite.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalPets = await _context.Pets.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.TotalAmount),
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                
                RecentOrders = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Pet)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync(),
                
                TopPets = await _context.Pets
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.IsFeatured)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                
                LowStockPets = await _context.Pets
                    .Include(p => p.Category)
                    .Where(p => p.IsActive && p.StockQuantity <= 2)
                    .OrderBy(p => p.StockQuantity)
                    .Take(5)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Admin/Pets
        public async Task<IActionResult> Pets(string search = "", int categoryId = 0, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.Pets.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || 
                                       p.Breed.Contains(search) ||
                                       p.Description.Contains(search));
            }

            if (categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var totalItems = await query.CountAsync();
            var pets = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(pets);
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Orders(string search = "", OrderStatus? status = null, int page = 1)
        {
            const int pageSize = 10;
            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pet)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.ShippingName.Contains(search) || 
                                       o.ShippingPhone.Contains(search) ||
                                       o.OrderNumber.Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var totalItems = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;
            ViewBag.Status = status;

            return View(orders);
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            foreach (var category in categories)
            {
                category.PetCount = await _context.Pets
                    .CountAsync(p => p.CategoryId == category.Id && p.IsActive);
            }

            return View(categories);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users(string search = "", int page = 1)
        {
            const int pageSize = 10;
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) || 
                                       u.Email.Contains(search) ||
                                       u.FullName.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.Search = search;

            return View(users);
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            order.Status = status;
            order.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        // POST: Admin/TogglePetStatus
        [HttpPost]
        public async Task<IActionResult> TogglePetStatus(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thú cưng" });
            }

            pet.IsActive = !pet.IsActive;
            pet.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = pet.IsActive ? "Đã kích hoạt thú cưng" : "Đã ẩn thú cưng",
                isActive = pet.IsActive 
            });
        }

        // POST: Admin/TogglePetFeatured
        [HttpPost]
        public async Task<IActionResult> TogglePetFeatured(int petId)
        {
            var pet = await _context.Pets.FindAsync(petId);
            if (pet == null)
            {
                return Json(new { success = false, message = "Không tìm thấy thú cưng" });
            }

            pet.IsFeatured = !pet.IsFeatured;
            pet.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Json(new { 
                success = true, 
                message = pet.IsFeatured ? "Đã đặt làm nổi bật" : "Đã bỏ nổi bật",
                isFeatured = pet.IsFeatured 
            });
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var viewModel = new AdminStatisticsViewModel
            {
                // Revenue by month (last 12 months)
                MonthlyRevenue = await GetMonthlyRevenue(),
                
                // Orders by status
                OrdersByStatus = await GetOrdersByStatus(),
                
                // Top selling pets
                TopSellingPets = await GetTopSellingPets(),
                
                // Category statistics
                CategoryStats = await GetCategoryStats()
            };

            return View(viewModel);
        }

        private async Task<List<MonthlyRevenueData>> GetMonthlyRevenue()
        {
            var result = new List<MonthlyRevenueData>();
            var startDate = DateTime.Now.AddMonths(-11).Date;

            for (int i = 0; i < 12; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1);

                var revenue = await _context.Orders
                    .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd && 
                               o.Status == OrderStatus.Delivered)
                    .SumAsync(o => o.TotalAmount);

                result.Add(new MonthlyRevenueData
                {
                    Month = monthStart.ToString("MM/yyyy"),
                    Revenue = revenue
                });
            }

            return result;
        }

        private async Task<Dictionary<string, int>> GetOrdersByStatus()
        {
            return await _context.Orders
                .GroupBy(o => o.Status)
                .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
        }

        private async Task<List<TopSellingPetData>> GetTopSellingPets()
        {
            return await _context.OrderDetails
                .Include(od => od.Pet)
                .GroupBy(od => od.Pet)
                .Select(g => new TopSellingPetData
                {
                    PetName = g.Key.Name,
                    TotalSold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.TotalPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .ToListAsync();
        }

        private async Task<List<CategoryStatsData>> GetCategoryStats()
        {
            return await _context.Categories
                .Select(c => new CategoryStatsData
                {
                    CategoryName = c.Name,
                    TotalPets = c.Pets.Count(p => p.IsActive),
                    TotalSold = c.Pets.SelectMany(p => p.OrderDetails).Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToListAsync();
        }

        // GET: Admin/GetUser/5
        [HttpGet]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phoneNumber = user.PhoneNumber,
                address = user.Address,
                dateOfBirth = user.DateOfBirth.ToString("yyyy-MM-dd"),
                isActive = user.IsActive
            });
        }

        // POST: Admin/UpdateUser
        [HttpPost]
        public async Task<IActionResult> UpdateUser(string id, string fullName, string email, string phoneNumber, string address, DateTime? dateOfBirth, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = fullName;
            user.Email = email;
            user.UserName = email;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
            user.DateOfBirth = dateOfBirth ?? default(DateTime);
            user.IsActive = isActive;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = result.Errors });
        }

        // POST: Admin/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string userId, bool isActive)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);
            
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = result.Errors });
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }

            return Json(new { success = false, errors = result.Errors });
        }
    }
}
