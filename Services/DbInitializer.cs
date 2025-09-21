using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;

namespace PetShopWebsite.Services;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRolesAsync(roleManager);

        // Seed admin user
        await SeedAdminUserAsync(userManager);

        // Seed categories
        await SeedCategoriesAsync(context);

        // Seed sample pets
        await SeedPetsAsync(context);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminEmail = "admin@petshop.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "Quản trị viên",
                PhoneNumber = "0123456789",
                Address = "123 Đường ABC, Quận 1, TP.HCM",
                DateOfBirth = new DateTime(1990, 1, 1),
                IsActive = true
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }

    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Name = "Chó",
                Description = "Các giống chó cưng đáng yêu",
                ImageUrl = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=300&h=200&fit=crop",
                DisplayOrder = 1,
                IsActive = true
            },
            new Category
            {
                Name = "Mèo",
                Description = "Các giống mèo cưng dễ thương",
                ImageUrl = "https://images.unsplash.com/photo-1574144611937-0df059b5ef3e?w=300&h=200&fit=crop",
                DisplayOrder = 2,
                IsActive = true
            },
            new Category
            {
                Name = "Chim",
                Description = "Các loài chim cảnh xinh đẹp",
                ImageUrl = "https://images.unsplash.com/photo-1544923408-75c5cef46f14?w=300&h=200&fit=crop",
                DisplayOrder = 3,
                IsActive = true
            },
            new Category
            {
                Name = "Cá cảnh",
                Description = "Các loài cá cảnh đẹp mắt",
                ImageUrl = "https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=300&h=200&fit=crop",
                DisplayOrder = 4,
                IsActive = true
            },
            new Category
            {
                Name = "Hamster",
                Description = "Hamster và các loài chuột cảnh",
                ImageUrl = "https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=300&h=200&fit=crop",
                DisplayOrder = 5,
                IsActive = true
            },
            new Category
            {
                Name = "Thỏ",
                Description = "Các giống thỏ cảnh đáng yêu",
                ImageUrl = "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=300&h=200&fit=crop",
                DisplayOrder = 6,
                IsActive = true
            }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedPetsAsync(ApplicationDbContext context)
    {
        if (await context.Pets.AnyAsync())
            return;

        var categories = await context.Categories.ToListAsync();
        var dogCategory = categories.First(c => c.Name == "Chó");
        var catCategory = categories.First(c => c.Name == "Mèo");
        var birdCategory = categories.First(c => c.Name == "Chim");
        var fishCategory = categories.First(c => c.Name == "Cá cảnh");
        var hamsterCategory = categories.First(c => c.Name == "Hamster");
        var rabbitCategory = categories.First(c => c.Name == "Thỏ");

        var pets = new List<Pet>
        {
            // Chó
            new Pet
            {
                Name = "Golden Retriever con",
                Breed = "Golden Retriever",
                Age = 3,
                Gender = "Đực",
                Weight = 2.5m,
                Color = "Vàng",
                Description = "Chó Golden Retriever con 3 tháng tuổi, rất thông minh và thân thiện. Đã tiêm phòng đầy đủ.",
                Price = 15000000,
                SalePrice = 13500000,
                StockQuantity = 5,
                MainImageUrl = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1551717743-49959800b1f6?w=500&h=400&fit=crop,https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500&h=400&fit=crop",
                IsFeatured = true,
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cho ăn 3 lần/ngày, tắm 1 tuần/lần, vận động thường xuyên",
                CategoryId = dogCategory.Id
            },
            new Pet
            {
                Name = "Poodle Toy",
                Breed = "Poodle",
                Age = 4,
                Gender = "Cái",
                Weight = 1.8m,
                Color = "Trắng",
                Description = "Poodle Toy 4 tháng tuổi, lông trắng mượt, rất dễ thương và ngoan ngoãn.",
                Price = 8000000,
                StockQuantity = 3,
                MainImageUrl = "https://images.unsplash.com/photo-1616190264687-b7ebf7aa2aa4?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1616190264687-b7ebf7aa2aa4?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=500&h=400&fit=crop",
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Chải lông hàng ngày, tắm 2 tuần/lần",
                CategoryId = dogCategory.Id
            },
            // Mèo
            new Pet
            {
                Name = "Mèo Ba Tư",
                Breed = "Persian",
                Age = 5,
                Gender = "Cái",
                Weight = 1.2m,
                Color = "Xám",
                Description = "Mèo Ba Tư 5 tháng tuổi, lông dài mượt mà, tính cách hiền lành.",
                Price = 12000000,
                SalePrice = 10800000,
                StockQuantity = 4,
                MainImageUrl = "https://images.unsplash.com/photo-1574144611937-0df059b5ef3e?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1574144611937-0df059b5ef3e?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=500&h=400&fit=crop",
                IsFeatured = true,
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Chải lông 2 lần/ngày, cho ăn thức ăn cao cấp",
                CategoryId = catCategory.Id
            },
            new Pet
            {
                Name = "Mèo Anh lông ngắn",
                Breed = "British Shorthair",
                Age = 6,
                Gender = "Đực",
                Weight = 1.5m,
                Color = "Xám xanh",
                Description = "Mèo Anh lông ngắn 6 tháng tuổi, màu xám xanh đặc trưng, rất thông minh.",
                Price = 18000000,
                StockQuantity = 2,
                MainImageUrl = "https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1573824267253-1c5d6b2e6d2e?w=500&h=400&fit=crop",
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cho ăn 2 lần/ngày, chải lông 1 tuần/lần",
                CategoryId = catCategory.Id
            },
            // Chim
            new Pet
            {
                Name = "Vẹt Cockatiel",
                Breed = "Cockatiel",
                Age = 8,
                Gender = "Đực",
                Weight = 0.1m,
                Color = "Vàng và xám",
                Description = "Vẹt Cockatiel 8 tháng tuổi, biết nói một số từ đơn giản, rất thân thiện.",
                Price = 3500000,
                StockQuantity = 6,
                MainImageUrl = "https://images.unsplash.com/photo-1544923408-75c5cef46f14?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1544923408-75c5cef46f14?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=500&h=400&fit=crop",
                IsFeatured = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cho ăn hạt chuyên dụng, thả bay trong nhà 2 giờ/ngày",
                CategoryId = birdCategory.Id
            },
            // Thêm chó khác
            new Pet
            {
                Name = "Husky Siberian",
                Breed = "Siberian Husky",
                Age = 4,
                Gender = "Đực",
                Weight = 3.2m,
                Color = "Đen trắng",
                Description = "Chó Husky Siberian 4 tháng tuổi, năng động và thông minh, phù hợp với gia đình yêu thể thao.",
                Price = 20000000,
                SalePrice = 18000000,
                StockQuantity = 3,
                MainImageUrl = "https://images.unsplash.com/photo-1605568427561-40dd23c2acea?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1605568427561-40dd23c2acea?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1605568427561-40dd23c2acea?w=500&h=400&fit=crop",
                IsFeatured = true,
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cần vận động nhiều, chải lông thường xuyên",
                CategoryId = dogCategory.Id
            },
            // Thêm mèo khác
            new Pet
            {
                Name = "Mèo Ragdoll",
                Breed = "Ragdoll",
                Age = 3,
                Gender = "Cái",
                Weight = 1.0m,
                Color = "Kem và nâu",
                Description = "Mèo Ragdoll 3 tháng tuổi, tính cách hiền lành và dễ bảo, rất thích được vuốt ve.",
                Price = 14000000,
                StockQuantity = 2,
                MainImageUrl = "https://images.unsplash.com/photo-1573824267253-1c5d6b2e6d2e?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1573824267253-1c5d6b2e6d2e?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=500&h=400&fit=crop",
                IsVaccinated = true,
                IsDewormed = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Chải lông hàng ngày, cho ăn thức ăn cao cấp",
                CategoryId = catCategory.Id
            },
            // Thêm thỏ
            new Pet
            {
                Name = "Thỏ Holland Lop",
                Breed = "Holland Lop",
                Age = 2,
                Gender = "Cái",
                Weight = 0.8m,
                Color = "Nâu và trắng",
                Description = "Thỏ Holland Lop 2 tháng tuổi, tai cụp đáng yêu, tính cách hiền lành và dễ chăm sóc.",
                Price = 2500000,
                StockQuantity = 5,
                MainImageUrl = "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=500&h=400&fit=crop",
                IsFeatured = true,
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cho ăn cỏ khô và rau củ tươi, vệ sinh chuồng thường xuyên",
                CategoryId = rabbitCategory.Id
            },
            // Thêm hamster
            new Pet
            {
                Name = "Hamster Golden",
                Breed = "Golden Hamster",
                Age = 1,
                Gender = "Đực",
                Weight = 0.1m,
                Color = "Vàng",
                Description = "Hamster Golden 1 tháng tuổi, rất dễ thương và dễ chăm sóc, phù hợp cho trẻ em.",
                Price = 500000,
                StockQuantity = 10,
                MainImageUrl = "https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=500&h=400&fit=crop&crop=face",
                ImageUrls = "https://images.unsplash.com/photo-1425082661705-1834bfd09dca?w=500&h=400&fit=crop&crop=face,https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=500&h=400&fit=crop",
                HealthStatus = "Khỏe mạnh",
                CareInstructions = "Cho ăn hạt chuyên dụng, thay cát vệ sinh hàng tuần",
                CategoryId = hamsterCategory.Id
            }
        };

        context.Pets.AddRange(pets);
        await context.SaveChangesAsync();
    }
}
