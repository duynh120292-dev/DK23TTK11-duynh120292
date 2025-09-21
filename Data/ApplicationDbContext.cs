using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Models;

namespace PetShopWebsite.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Pet> Pets { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Review> Reviews { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<Pet>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Pets)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CartItem>()
            .HasOne(c => c.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasOne(c => c.Pet)
            .WithMany(p => p.CartItems)
            .HasForeignKey(c => c.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Order)
            .WithMany(o => o.OrderDetails)
            .HasForeignKey(od => od.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderDetail>()
            .HasOne(od => od.Pet)
            .WithMany(p => p.OrderDetails)
            .HasForeignKey(od => od.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Review>()
            .HasOne(r => r.Pet)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure indexes
        builder.Entity<Pet>()
            .HasIndex(p => p.CategoryId);

        builder.Entity<Pet>()
            .HasIndex(p => new { p.IsActive, p.IsFeatured });

        builder.Entity<Pet>()
            .HasIndex(p => p.Price);

        builder.Entity<Order>()
            .HasIndex(o => new { o.UserId, o.Status });

        builder.Entity<Order>()
            .HasIndex(o => o.OrderDate);

        builder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        // Configure constraints using ToTable
        builder.Entity<Pet>()
            .ToTable(t => t.HasCheckConstraint("CK_Pet_Price", "Price > 0"));

        builder.Entity<Pet>()
            .ToTable(t => t.HasCheckConstraint("CK_Pet_SalePrice", "SalePrice IS NULL OR SalePrice >= 0"));

        builder.Entity<Pet>()
            .ToTable(t => t.HasCheckConstraint("CK_Pet_StockQuantity", "StockQuantity >= 0"));

        builder.Entity<Pet>()
            .ToTable(t => t.HasCheckConstraint("CK_Pet_Age", "Age > 0 AND Age <= 300"));

        builder.Entity<Pet>()
            .ToTable(t => t.HasCheckConstraint("CK_Pet_Weight", "Weight > 0"));

        builder.Entity<CartItem>()
            .ToTable(t => t.HasCheckConstraint("CK_CartItem_Quantity", "Quantity > 0"));

        builder.Entity<OrderDetail>()
            .ToTable(t => t.HasCheckConstraint("CK_OrderDetail_Quantity", "Quantity > 0"));

        builder.Entity<Review>()
            .ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "Rating >= 1 AND Rating <= 5"));
    }
}
