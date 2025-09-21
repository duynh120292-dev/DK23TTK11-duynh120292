using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;
using PetShopWebsite.ViewModels;

namespace PetShopWebsite.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<CartController> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: Cart
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _context.CartItems
            .Include(c => c.Pet)
                .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var cartViewModel = new CartViewModel
        {
            Items = cartItems.Select(c => new CartItemViewModel
            {
                Id = c.Id,
                Pet = c.Pet,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice
            }).ToList()
        };

        return View(cartViewModel);
    }

    // POST: Cart/AddToCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int petId, int quantity = 1)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
        }

        var pet = await _context.Pets.FindAsync(petId);
        if (pet == null || !pet.IsActive)
        {
            return Json(new { success = false, message = "Thú cưng không tồn tại" });
        }

        if (pet.StockQuantity < quantity)
        {
            return Json(new { success = false, message = "Không đủ số lượng trong kho" });
        }

        // Check if item already exists in cart
        var existingCartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.UserId == userId && c.PetId == petId);

        if (existingCartItem != null)
        {
            // Update quantity
            var newQuantity = existingCartItem.Quantity + quantity;
            if (newQuantity > pet.StockQuantity)
            {
                return Json(new { success = false, message = "Không đủ số lượng trong kho" });
            }

            existingCartItem.Quantity = newQuantity;
            existingCartItem.UnitPrice = pet.SalePrice ?? pet.Price;
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                UserId = userId,
                PetId = petId,
                Quantity = quantity,
                UnitPrice = pet.SalePrice ?? pet.Price
            };

            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();

        // Get cart count for response
        var cartCount = await _context.CartItems
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);

        return Json(new { success = true, message = "Đã thêm vào giỏ hàng", cartCount = cartCount });
    }

    // POST: Cart/UpdateQuantity
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var cartItem = await _context.CartItems
            .Include(c => c.Pet)
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
        }

        if (quantity <= 0)
        {
            return Json(new { success = false, message = "Số lượng phải lớn hơn 0" });
        }

        if (quantity > cartItem.Pet.StockQuantity)
        {
            return Json(new { success = false, message = "Không đủ số lượng trong kho" });
        }

        cartItem.Quantity = quantity;
        await _context.SaveChangesAsync();

        var totalPrice = cartItem.UnitPrice * cartItem.Quantity;

        return Json(new { 
            success = true, 
            message = "Đã cập nhật số lượng",
            totalPrice = totalPrice.ToString("N0"),
            cartTotal = await GetCartTotalAsync(userId)
        });
    }

    // POST: Cart/RemoveItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int cartItemId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập" });
        }

        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

        if (cartItem == null)
        {
            return Json(new { success = false, message = "Không tìm thấy sản phẩm trong giỏ hàng" });
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            message = "Đã xóa sản phẩm khỏi giỏ hàng",
            cartTotal = await GetCartTotalAsync(userId)
        });
    }

    // POST: Cart/ClearCart
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCart()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        _context.CartItems.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Đã xóa tất cả sản phẩm khỏi giỏ hàng";
        return RedirectToAction(nameof(Index));
    }

    // GET: Cart/GetCartCount
    [HttpGet]
    public async Task<IActionResult> GetCartCount()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return Json(new { count = 0 });
        }

        var count = await _context.CartItems
            .Where(c => c.UserId == userId)
            .SumAsync(c => c.Quantity);

        return Json(new { count = count });
    }

    // GET: Cart/Checkout
    public async Task<IActionResult> Checkout()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var cartItems = await _context.CartItems
            .Include(c => c.Pet)
                .ThenInclude(p => p.Category)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["Error"] = "Giỏ hàng của bạn đang trống";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.GetUserAsync(User);
        var cartViewModel = new CartViewModel
        {
            Items = cartItems.Select(c => new CartItemViewModel
            {
                Id = c.Id,
                Pet = c.Pet,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice
            }).ToList()
        };

        return View(cartViewModel);
    }

    #region Helper Methods

    private async Task<string> GetCartTotalAsync(string userId)
    {
        var cartItems = await _context.CartItems
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var subTotal = cartItems.Sum(c => c.UnitPrice * c.Quantity);
        var shippingFee = 50000m; // 50k shipping fee
        var total = subTotal + shippingFee;

        return total.ToString("N0");
    }

    #endregion
}
