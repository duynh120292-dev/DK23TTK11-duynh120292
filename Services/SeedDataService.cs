using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PetShopWebsite.Data;
using PetShopWebsite.Models;

namespace PetShopWebsite.Services
{
    public class SeedDataService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedDataService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            // Ensure database is created
            await _context.Database.EnsureCreatedAsync();

            // Seed roles
            await SeedRolesAsync();

            // Seed admin user
            await SeedAdminUserAsync();

            // Seed categories if not exist
            await SeedCategoriesAsync();

            // Seed sample pets if not exist
            await SeedPetsAsync();
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@petshop.com";
            const string adminPassword = "Admin123!";

            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Quản Trị Viên",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội, Việt Nam",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 23)
                };

                var result = await _userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ Admin user created successfully: {adminEmail} / {adminPassword}");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure admin user has Admin role
                if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine($"✅ Added Admin role to existing user: {adminEmail}");
                }
                else
                {
                    Console.WriteLine($"✅ Admin user already exists: {adminEmail}");
                }
            }
        }

        private async Task SeedCategoriesAsync()
        {
            if (await _context.Categories.AnyAsync())
                return;

            var categories = new[]
            {
                new Category
                {
                    Name = "Chó",
                    Description = "Các giống chó cưng đáng yêu và thông minh",
                    ImageUrl = "/images/categories/dog.jpg",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 23)
                },
                new Category
                {
                    Name = "Mèo",
                    Description = "Các giống mèo dễ thương và độc lập",
                    ImageUrl = "/images/categories/cat.jpg",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 8, 23)
                },
                new Category
                {
                    Name = "Chim",
                    Description = "Các loài chim cảnh xinh đẹp và biết hót",
                    ImageUrl = "/images/categories/bird.jpg",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Category
                {
                    Name = "Hamster",
                    Description = "Chuột hamster nhỏ xinh và dễ nuôi",
                    ImageUrl = "/images/categories/hamster.jpg",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Category
                {
                    Name = "Thỏ",
                    Description = "Thỏ cảnh dễ thương và hiền lành",
                    ImageUrl = "/images/categories/rabbit.jpg",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new Category
                {
                    Name = "Cá",
                    Description = "Cá cảnh đẹp mắt cho hồ cá",
                    ImageUrl = "/images/categories/fish.jpg",
                    DisplayOrder = 6,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            _context.Categories.AddRange(categories);
            await _context.SaveChangesAsync();
        }

        private async Task SeedPetsAsync()
        {
            if (await _context.Pets.AnyAsync())
                return;

            var categories = await _context.Categories.ToListAsync();
            var dogCategory = categories.First(c => c.Name == "Chó");
            var catCategory = categories.First(c => c.Name == "Mèo");

            var pets = new[]
            {
                new Pet
                {
                    Name = "Golden Retriever",
                    Breed = "Golden Retriever",
                    Age = 3,
                    Gender = "Đực",
                    Color = "Vàng",
                    Weight = 25.5m,
                    Price = 15000000,
                    SalePrice = 12000000,
                    Description = "Chó Golden Retriever thông minh, thân thiện và dễ huấn luyện. Rất phù hợp với gia đình có trẻ em.",
                    HealthStatus = "Khỏe mạnh",
                    IsVaccinated = true,
                    IsDewormed = true,
                    IsFeatured = true,
                    IsActive = true,
                    StockQuantity = 5,
                    CategoryId = dogCategory.Id,
                    MainImageUrl = "/images/pets/golden-retriever.jpg",
                    ImageUrls = "['/images/pets/golden-retriever-1.jpg','/images/pets/golden-retriever-2.jpg']",
                    CareInstructions = "Cần tắm rửa thường xuyên, chải lông hàng ngày, tập thể dục đều đặn.",
                    CreatedAt = new DateTime(2025, 8, 23)
                },
                new Pet
                {
                    Name = "Mèo Ba Tư",
                    Breed = "Persian",
                    Age = 2,
                    Gender = "Cái",
                    Color = "Trắng",
                    Weight = 4.2m,
                    Price = 8000000,
                    Description = "Mèo Ba Tư với bộ lông dài mềm mại, tính cách hiền lành và thích được vuốt ve.",
                    HealthStatus = "Khỏe mạnh",
                    IsVaccinated = true,
                    IsDewormed = true,
                    IsFeatured = true,
                    IsActive = true,
                    StockQuantity = 3,
                    CategoryId = catCategory.Id,
                    MainImageUrl = "/images/pets/persian-cat.jpg",
                    ImageUrls = "['/images/pets/persian-cat-1.jpg','/images/pets/persian-cat-2.jpg']",
                    CareInstructions = "Cần chải lông hàng ngày, vệ sinh mắt thường xuyên, cho ăn thức ăn chất lượng cao.",
                    CreatedAt = DateTime.Now
                },
                new Pet
                {
                    Name = "Poodle Toy",
                    Breed = "Poodle",
                    Age = 1,
                    Gender = "Đực",
                    Color = "Nâu",
                    Weight = 3.0m,
                    Price = 12000000,
                    SalePrice = 10000000,
                    Description = "Poodle Toy nhỏ xinh, thông minh và rất trung thành với chủ.",
                    HealthStatus = "Khỏe mạnh",
                    IsVaccinated = true,
                    IsDewormed = true,
                    IsFeatured = false,
                    IsActive = true,
                    StockQuantity = 2,
                    CategoryId = dogCategory.Id,
                    MainImageUrl = "/images/pets/poodle-toy.jpg",
                    ImageUrls = "['/images/pets/poodle-toy-1.jpg']",
                    CareInstructions = "Cần cắt tỉa lông định kỳ, tập thể dục nhẹ nhàng, chăm sóc răng miệng.",
                    CreatedAt = DateTime.Now
                },
                new Pet
                {
                    Name = "Mèo Anh Lông Ngắn",
                    Breed = "British Shorthair",
                    Age = 4,
                    Gender = "Đực",
                    Color = "Xám",
                    Weight = 5.8m,
                    Price = 6000000,
                    Description = "Mèo Anh lông ngắn với thân hình chắc nịch, tính cách độc lập nhưng rất tình cảm.",
                    HealthStatus = "Khỏe mạnh",
                    IsVaccinated = true,
                    IsDewormed = true,
                    IsFeatured = false,
                    IsActive = true,
                    StockQuantity = 4,
                    CategoryId = catCategory.Id,
                    MainImageUrl = "/images/pets/british-shorthair.jpg",
                    ImageUrls = "['/images/pets/british-shorthair-1.jpg','/images/pets/british-shorthair-2.jpg']",
                    CareInstructions = "Chải lông 2-3 lần/tuần, kiểm tra sức khỏe định kỳ, cho ăn đúng giờ.",
                    CreatedAt = DateTime.Now
                },
                new Pet
                {
                    Name = "Husky Siberian",
                    Breed = "Siberian Husky",
                    Age = 2,
                    Gender = "Cái",
                    Color = "Đen trắng",
                    Weight = 22.0m,
                    Price = 18000000,
                    SalePrice = 15000000,
                    Description = "Husky Siberian năng động, thông minh và có sức bền cao. Cần nhiều vận động.",
                    HealthStatus = "Khỏe mạnh",
                    IsVaccinated = true,
                    IsDewormed = true,
                    IsFeatured = true,
                    IsActive = true,
                    StockQuantity = 1,
                    CategoryId = dogCategory.Id,
                    MainImageUrl = "/images/pets/siberian-husky.jpg",
                    ImageUrls = "['/images/pets/siberian-husky-1.jpg','/images/pets/siberian-husky-2.jpg']",
                    CareInstructions = "Cần tập thể dục nhiều, chải lông thường xuyên, môi trường mát mẻ.",
                    CreatedAt = DateTime.Now
                }
            };

            _context.Pets.AddRange(pets);
            await _context.SaveChangesAsync();
        }
    }
}
