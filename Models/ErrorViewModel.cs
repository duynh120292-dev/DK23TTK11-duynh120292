using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetShopWebsite.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}

// User model extending IdentityUser
public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    public DateTime DateOfBirth { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

// Category model
public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty; // Chó, Mèo, Chim, Cá, Hamster, v.v.

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    // Navigation properties
    public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();

    // Calculated property
    [NotMapped]
    public int PetCount { get; set; }
}

// Pet model (Sản phẩm thú cưng)
public class Pet
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Breed { get; set; } = string.Empty; // Giống: Golden Retriever, Persian, v.v.

    [Range(1, 300)]
    public int Age { get; set; } // Tuổi (tháng)

    [Required]
    [StringLength(20)]
    public string Gender { get; set; } = string.Empty; // Đực/Cái

    [Range(0.1, 100)]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; } // Cân nặng (kg)

    [Required]
    [StringLength(50)]
    public string Color { get; set; } = string.Empty; // Màu lông

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalePrice { get; set; } // Giá khuyến mãi

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; } // Số lượng tồn kho

    [StringLength(200)]
    public string? MainImageUrl { get; set; }

    public string? ImageUrls { get; set; } // JSON array của URLs

    public bool IsFeatured { get; set; } = false; // Sản phẩm nổi bật

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    public DateTime? UpdatedAt { get; set; }

    // Health & Care Info
    public bool IsVaccinated { get; set; } = false; // Đã tiêm phòng

    public bool IsDewormed { get; set; } = false; // Đã tẩy giun

    [StringLength(200)]
    public string? HealthStatus { get; set; } // Tình trạng sức khỏe

    [StringLength(1000)]
    public string? CareInstructions { get; set; } // Hướng dẫn chăm sóc

    // Foreign Keys
    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}

// CartItem model (Giỏ hàng)
public class CartItem
{
    public int Id { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int PetId { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;

    // Calculated property
    [NotMapped]
    public decimal TotalPrice => UnitPrice * Quantity;
}

// Order model (Đơn hàng)
public class Order
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string OrderNumber { get; set; } = string.Empty; // Mã đơn hàng

    public DateTime OrderDate { get; set; } = DateTime.Now;

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ShippingFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    // Shipping Info
    [Required]
    [StringLength(100)]
    public string ShippingName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Payment Info
    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    public DateTime? PaymentDate { get; set; }

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}

// OrderDetail model (Chi tiết đơn hàng)
public class OrderDetail
{
    public int Id { get; set; }

    [Range(1, 99)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Foreign Keys
    [Required]
    public int OrderId { get; set; }

    [Required]
    public int PetId { get; set; }

    // Navigation properties
    public virtual Order Order { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}

// Review model (Đánh giá)
public class Review
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; } // 1-5 sao

    [StringLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = new DateTime(2025, 8, 23);

    public bool IsApproved { get; set; } = false;

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int PetId { get; set; }

    // Navigation properties
    public virtual ApplicationUser User { get; set; } = null!;
    public virtual Pet Pet { get; set; } = null!;
}

// Enums
public enum OrderStatus
{
    Pending = 0,        // Chờ xử lý
    Confirmed = 1,      // Đã xác nhận
    Processing = 2,     // Đang chuẩn bị
    Shipping = 3,       // Đang giao hàng
    Delivered = 4,      // Đã giao hàng
    Cancelled = 5,      // Đã hủy
    Returned = 6        // Đã trả hàng
}

public enum PaymentMethod
{
    COD = 0,           // Thanh toán khi nhận hàng
    BankTransfer = 1,  // Chuyển khoản ngân hàng
    CreditCard = 2,    // Thẻ tín dụng
    EWallet = 3        // Ví điện tử
}

public enum PaymentStatus
{
    Pending = 0,       // Chờ thanh toán
    Paid = 1,          // Đã thanh toán
    Failed = 2,        // Thanh toán thất bại
    Refunded = 3       // Đã hoàn tiền
}
