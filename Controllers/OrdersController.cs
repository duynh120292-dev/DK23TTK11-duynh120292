using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace PetShopWebsite.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Orders
        public async Task<IActionResult> Index(int page = 1)
        {
            var userId = _userManager.GetUserId(User);
            const int pageSize = 10;

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pet)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate);

            var totalItems = await query.CountAsync();
            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.TotalItems = totalItems;

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pet)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Pet)
                .ThenInclude(p => p.Category)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index", "Cart");
            }

            var user = await _userManager.GetUserAsync(User);
            var model = new CheckoutViewModel
            {
                CartItems = cartItems,
                CustomerName = user?.FullName ?? "",
                CustomerEmail = user?.Email ?? "",
                CustomerPhone = user?.PhoneNumber ?? "",
                CustomerAddress = user?.Address ?? ""
            };

            return View(model);
        }

        // POST: Orders/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var cartItems = await _context.CartItems
                .Include(c => c.Pet)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index", "Cart");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create order
                    var order = new Order
                    {
                        UserId = userId,
                        ShippingName = model.CustomerName,
                        ShippingPhone = model.CustomerPhone,
                        ShippingAddress = model.CustomerAddress,
                        PaymentMethod = Enum.Parse<PaymentMethod>(model.PaymentMethod),
                        Notes = model.Notes,
                        SubTotal = cartItems.Sum(c => c.UnitPrice * c.Quantity),
                        TotalAmount = cartItems.Sum(c => c.UnitPrice * c.Quantity) * 1.1m,
                        Status = OrderStatus.Pending,
                        OrderDate = DateTime.Now
                    };

                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Create order details
                    foreach (var cartItem in cartItems)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.Id,
                            PetId = cartItem.PetId,
                            Quantity = cartItem.Quantity,
                            UnitPrice = cartItem.UnitPrice,
                            TotalPrice = cartItem.UnitPrice * cartItem.Quantity
                        };

                        _context.OrderDetails.Add(orderDetail);

                        // Update pet stock
                        var pet = cartItem.Pet;
                        if (pet != null)
                        {
                            pet.StockQuantity -= cartItem.Quantity;
                            if (pet.StockQuantity <= 0)
                            {
                                pet.IsActive = false;
                            }
                        }
                    }

                    // Clear cart
                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["Success"] = "Đặt hàng thành công! Chúng tôi sẽ liên hệ với bạn sớm nhất.";
                    return RedirectToAction("Details", new { id = order.Id });
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                }
            }

            // If we got this far, something failed, redisplay form
            model.CartItems = cartItems;
            return View(model);
        }

        // POST: Orders/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Pet)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            if (order.Status != OrderStatus.Pending)
            {
                return Json(new { success = false, message = "Không thể hủy đơn hàng này" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Restore pet stock
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Pet != null)
                    {
                        orderDetail.Pet.StockQuantity += orderDetail.Quantity;
                        orderDetail.Pet.IsActive = true;
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new { success = true, message = "Đã hủy đơn hàng thành công" });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = "Có lỗi xảy ra khi hủy đơn hàng" });
            }
        }
    }

    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; } = new();
        
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        public string CustomerName { get; set; } = "";
        
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string CustomerEmail { get; set; } = "";
        
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string CustomerPhone { get; set; } = "";
        
        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string CustomerAddress { get; set; } = "";
        
        [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
        public string PaymentMethod { get; set; } = "";
        
        public string? Notes { get; set; }
        
        public decimal SubTotal => CartItems.Sum(c => c.UnitPrice * c.Quantity);
        public decimal TaxAmount => SubTotal * 0.1m;
        public decimal TotalAmount => SubTotal + TaxAmount;
    }
}
