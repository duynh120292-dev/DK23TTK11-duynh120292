using System.ComponentModel.DataAnnotations;
using PetShopWebsite.Models;

namespace PetShopWebsite.ViewModels;

public class PetListViewModel
{
    public IEnumerable<Pet> Pets { get; set; } = new List<Pet>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
    public int? CurrentCategoryId { get; set; }
    public string? SearchQuery { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? SortBy { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; } = 0;
}

public class PetDetailsViewModel
{
    public Pet Pet { get; set; } = null!;
    public IEnumerable<Pet> RelatedPets { get; set; } = new List<Pet>();
    public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
    public bool CanReview { get; set; } = false;
}

public class AddReviewViewModel
{
    public int PetId { get; set; }
    
    [Required(ErrorMessage = "Vui lòng chọn số sao")]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
    [Display(Name = "Đánh giá")]
    public int Rating { get; set; }
    
    [StringLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
    [Display(Name = "Bình luận")]
    public string? Comment { get; set; }
}

public class CartItemViewModel
{
    public int Id { get; set; }
    public Pet Pet { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => UnitPrice * Quantity;
}

public class CartViewModel
{
    public IEnumerable<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
    public decimal SubTotal => Items.Sum(i => i.TotalPrice);
    public decimal ShippingFee { get; set; } = 50000; // 50k shipping fee
    public decimal TotalAmount => SubTotal + ShippingFee;
    public int TotalItems => Items.Sum(i => i.Quantity);
}

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Họ tên người nhận là bắt buộc")]
    [StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
    [Display(Name = "Họ tên người nhận")]
    public string ShippingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự")]
    [Display(Name = "Địa chỉ giao hàng")]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    [Display(Name = "Ghi chú")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; }

    // Cart summary
    public CartViewModel Cart { get; set; } = new CartViewModel();
}

public class OrderViewModel
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string ShippingName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public IEnumerable<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
}

// Admin ViewModels
public class AdminDashboardViewModel
{
    public int TotalPets { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingOrders { get; set; }

    public List<Order> RecentOrders { get; set; } = new();
    public List<Pet> TopPets { get; set; } = new();
    public List<Pet> LowStockPets { get; set; } = new();
}

public class AdminStatisticsViewModel
{
    public List<MonthlyRevenueData> MonthlyRevenue { get; set; } = new();
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public List<TopSellingPetData> TopSellingPets { get; set; } = new();
    public List<CategoryStatsData> CategoryStats { get; set; } = new();
}

public class MonthlyRevenueData
{
    public string Month { get; set; } = "";
    public decimal Revenue { get; set; }
}

public class TopSellingPetData
{
    public string PetName { get; set; } = "";
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class CategoryStatsData
{
    public string CategoryName { get; set; } = "";
    public int TotalPets { get; set; }
    public int TotalSold { get; set; }
}

public class OrderDetailViewModel
{
    public Pet Pet { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderListViewModel
{
    public IEnumerable<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>();
    public OrderStatus? FilterStatus { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalItems { get; set; } = 0;
}



public class AdminPetViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên thú cưng là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tên không được vượt quá 100 ký tự")]
    [Display(Name = "Tên thú cưng")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Giống là bắt buộc")]
    [StringLength(100, ErrorMessage = "Giống không được vượt quá 100 ký tự")]
    [Display(Name = "Giống")]
    public string Breed { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tuổi là bắt buộc")]
    [Range(1, 300, ErrorMessage = "Tuổi phải từ 1 đến 300 tháng")]
    [Display(Name = "Tuổi (tháng)")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Giới tính là bắt buộc")]
    [Display(Name = "Giới tính")]
    public string Gender { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cân nặng là bắt buộc")]
    [Range(0.1, 100, ErrorMessage = "Cân nặng phải từ 0.1 đến 100 kg")]
    [Display(Name = "Cân nặng (kg)")]
    public decimal Weight { get; set; }

    [Required(ErrorMessage = "Màu sắc là bắt buộc")]
    [StringLength(50, ErrorMessage = "Màu sắc không được vượt quá 50 ký tự")]
    [Display(Name = "Màu sắc")]
    public string Color { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    [Display(Name = "Giá (VNĐ)")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Giá khuyến mãi phải lớn hơn hoặc bằng 0")]
    [Display(Name = "Giá khuyến mãi (VNĐ)")]
    public decimal? SalePrice { get; set; }

    [Required(ErrorMessage = "Số lượng tồn kho là bắt buộc")]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
    [Display(Name = "Số lượng tồn kho")]
    public int StockQuantity { get; set; }

    [Display(Name = "Hình ảnh chính")]
    public string? MainImageUrl { get; set; }

    [Display(Name = "Sản phẩm nổi bật")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Đã tiêm phòng")]
    public bool IsVaccinated { get; set; }

    [Display(Name = "Đã tẩy giun")]
    public bool IsDewormed { get; set; }

    [StringLength(200, ErrorMessage = "Tình trạng sức khỏe không được vượt quá 200 ký tự")]
    [Display(Name = "Tình trạng sức khỏe")]
    public string? HealthStatus { get; set; }

    [StringLength(1000, ErrorMessage = "Hướng dẫn chăm sóc không được vượt quá 1000 ký tự")]
    [Display(Name = "Hướng dẫn chăm sóc")]
    public string? CareInstructions { get; set; }

    [Required(ErrorMessage = "Danh mục là bắt buộc")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    [Display(Name = "Kích hoạt")]
    public bool IsActive { get; set; } = true;

    // For dropdown
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();
}
