using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PetShopWebsite.Models;

namespace PetShopWebsite.Controllers
{
    public class SetupController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SetupController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Setup/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Setup/CreateAdmin
        [HttpPost]
        public async Task<IActionResult> CreateAdmin(string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            // Check if admin role exists, create if not
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email này đã được sử dụng";
                return View();
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = "Quản Trị Viên",
                PhoneNumber = "0123456789",
                Address = "Hà Nội, Việt Nam",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                ViewBag.Success = $"Tài khoản admin đã được tạo thành công! Email: {email}";
                return View();
            }
            else
            {
                ViewBag.Error = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                return View();
            }
        }

        // GET: Setup/TestAdmin
        public async Task<IActionResult> TestAdmin()
        {
            var adminUsers = new List<object>();

            // Get all users with Admin role
            var usersInAdminRole = await _userManager.GetUsersInRoleAsync("Admin");

            foreach (var user in usersInAdminRole)
            {
                adminUsers.Add(new
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    IsActive = user.IsActive,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt
                });
            }

            ViewBag.AdminUsers = adminUsers;
            return View();
        }

        // POST: Setup/ResetAdmin
        [HttpPost]
        public async Task<IActionResult> ResetAdmin()
        {
            try
            {
                // Delete existing admin user
                var existingAdmin = await _userManager.FindByEmailAsync("admin@petshop.com");
                if (existingAdmin != null)
                {
                    await _userManager.DeleteAsync(existingAdmin);
                }

                // Ensure roles exist
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                // Create new admin user
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@petshop.com",
                    Email = "admin@petshop.com",
                    EmailConfirmed = true,
                    FullName = "Quản Trị Viên",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội, Việt Nam",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    ViewBag.Success = "Tài khoản admin đã được reset và tạo lại thành công!";
                }
                else
                {
                    ViewBag.Error = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }

        // GET: Setup/DebugLogin
        public async Task<IActionResult> DebugLogin(string email = "admin@petshop.com", string password = "Admin123!")
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ViewBag.Result = "User not found";
                return View();
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.Result = $@"
User Found: {user.Email}
Password Check: {passwordCheck}
Email Confirmed: {user.EmailConfirmed}
Is Active: {user.IsActive}
Lockout Enabled: {user.LockoutEnabled}
Lockout End: {user.LockoutEnd}
Roles: {string.Join(", ", roles)}
Created: {user.CreatedAt}
";

            return View();
        }

        // GET: Setup/ForceCreateAdmin
        public async Task<IActionResult> ForceCreateAdmin()
        {
            try
            {
                // Delete ALL existing users first
                var allUsers = _userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    await _userManager.DeleteAsync(user);
                }

                // Ensure roles exist
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create new admin user with simple password
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@petshop.com",
                    Email = "admin@petshop.com",
                    NormalizedUserName = "ADMIN@PETSHOP.COM",
                    NormalizedEmail = "ADMIN@PETSHOP.COM",
                    EmailConfirmed = true,
                    FullName = "Admin",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội",
                    IsActive = true,
                    LockoutEnabled = false,
                    CreatedAt = DateTime.Now
                };

                // Try with simpler password first
                var result = await _userManager.CreateAsync(adminUser, "123456");
                if (!result.Succeeded)
                {
                    // Try with original password
                    result = await _userManager.CreateAsync(adminUser, "Admin123!");
                }

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    ViewBag.Success = $@"
✅ Tài khoản admin đã được tạo thành công!

📧 Email: admin@petshop.com
🔑 Mật khẩu: 123456 (hoặc Admin123!)

🔗 Đăng nhập tại: /Identity/Account/Login
🎛️ Trang admin: /Admin
";
                }
                else
                {
                    ViewBag.Error = $"Lỗi: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Exception: {ex.Message}";
            }

            return View();
        }

        // GET: Setup/CreateAdminSimple
        public async Task<IActionResult> CreateAdminSimple()
        {
            try
            {
                // Ensure roles exist
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Delete existing admin users if exist
                var existingUser1 = await _userManager.FindByNameAsync("admin");
                if (existingUser1 != null)
                {
                    await _userManager.DeleteAsync(existingUser1);
                }

                var existingUser2 = await _userManager.FindByEmailAsync("admin@admin.com");
                if (existingUser2 != null)
                {
                    await _userManager.DeleteAsync(existingUser2);
                }

                // Create new admin user with email as username
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    NormalizedUserName = "ADMIN@ADMIN.COM",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    EmailConfirmed = true,
                    FullName = "Admin",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsActive = true,
                    LockoutEnabled = false,
                    CreatedAt = DateTime.Now
                };

                // Create with simple password
                var result = await _userManager.CreateAsync(adminUser, "admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    ViewBag.Success = $@"
✅ Tài khoản admin đã được tạo thành công!

📧 Email: admin@admin.com
🔑 Password: admin@123

🔗 Đăng nhập tại: /Identity/Account/Login
🎛️ Trang admin: /Admin

Lưu ý: Sử dụng email admin@admin.com để đăng nhập
";
                }
                else
                {
                    ViewBag.Error = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }

        // GET: Setup/ForceResetAdmin
        public async Task<IActionResult> ForceResetAdmin()
        {
            try
            {
                // Delete ALL existing users
                var allUsers = _userManager.Users.ToList();
                foreach (var user in allUsers)
                {
                    await _userManager.DeleteAsync(user);
                }

                // Ensure roles exist
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Create new admin user with simple credentials
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@admin.com",
                    Email = "admin@admin.com",
                    NormalizedUserName = "ADMIN@ADMIN.COM",
                    NormalizedEmail = "ADMIN@ADMIN.COM",
                    EmailConfirmed = true,
                    FullName = "Admin",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsActive = true,
                    LockoutEnabled = false,
                    CreatedAt = DateTime.Now
                };

                // Create with simple password
                var result = await _userManager.CreateAsync(adminUser, "admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    ViewBag.Success = $@"
✅ Tài khoản admin đã được tạo thành công!

📧 Email: admin@admin.com
🔑 Password: admin@123

🔗 Đăng nhập tại: /Identity/Account/Login
🎛️ Trang admin: /Admin

⚠️ Tất cả user cũ đã bị xóa!
";
                }
                else
                {
                    ViewBag.Error = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }

        // GET: Setup/CreateCustomAdmin
        public async Task<IActionResult> CreateCustomAdmin()
        {
            try
            {
                // Ensure roles exist
                var roles = new[] { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                // Delete existing admin user if exists
                var existingUser = await _userManager.FindByEmailAsync("iadmin@gmail.com");
                if (existingUser != null)
                {
                    await _userManager.DeleteAsync(existingUser);
                }

                // Create new admin user with custom credentials
                var adminUser = new ApplicationUser
                {
                    UserName = "iadmin@gmail.com",
                    Email = "iadmin@gmail.com",
                    NormalizedUserName = "IADMIN@GMAIL.COM",
                    NormalizedEmail = "IADMIN@GMAIL.COM",
                    EmailConfirmed = true,
                    FullName = "Quản Trị Viên",
                    PhoneNumber = "0123456789",
                    Address = "Hà Nội",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsActive = true,
                    LockoutEnabled = false,
                    CreatedAt = DateTime.Now
                };

                // Create with custom password
                var result = await _userManager.CreateAsync(adminUser, "admin@123");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");

                    ViewBag.Success = $@"
✅ Tài khoản admin đã được tạo thành công!

📧 Email: iadmin@gmail.com
🔑 Password: admin@123

🔗 Đăng nhập tại: /Identity/Account/Login
🎛️ Trang admin: /Admin

Bạn có thể đăng nhập ngay bây giờ!
";
                }
                else
                {
                    ViewBag.Error = $"Lỗi tạo tài khoản: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }

        // GET: Setup/FixAdminRole
        public async Task<IActionResult> FixAdminRole()
        {
            try
            {
                // Find the user
                var user = await _userManager.FindByEmailAsync("iadmin@gmail.com");
                if (user == null)
                {
                    ViewBag.Error = "User iadmin@gmail.com not found!";
                    return View("TestAdmin");
                }

                // Check if Admin role exists
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Remove user from all roles first
                var userRoles = await _userManager.GetRolesAsync(user);
                if (userRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                }

                // Add user to Admin role
                var result = await _userManager.AddToRoleAsync(user, "Admin");

                if (result.Succeeded)
                {
                    // Verify the role assignment
                    var roles = await _userManager.GetRolesAsync(user);

                    ViewBag.Success = $@"
✅ Admin role đã được gán thành công!

📧 Email: iadmin@gmail.com
🔑 Password: admin@123
👤 Roles: {string.Join(", ", roles)}

🔗 Đăng nhập tại: /Identity/Account/Login
🎛️ Trang admin: /Admin

Bây giờ bạn có thể đăng nhập!
";
                }
                else
                {
                    ViewBag.Error = $"Lỗi gán role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }

        // GET: Setup/CheckUserRoles
        public async Task<IActionResult> CheckUserRoles()
        {
            try
            {
                var user = await _userManager.FindByEmailAsync("iadmin@gmail.com");
                if (user == null)
                {
                    ViewBag.Error = "User not found!";
                    return View("TestAdmin");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var isInAdminRole = await _userManager.IsInRoleAsync(user, "Admin");
                var allRoles = _roleManager.Roles.ToList();

                ViewBag.Success = $@"
📊 Thông tin User & Roles:

👤 User: {user.Email}
🆔 User ID: {user.Id}
✅ Email Confirmed: {user.EmailConfirmed}
🔓 Lockout Enabled: {user.LockoutEnabled}
📅 Created: {user.CreatedAt}

🎭 User Roles: {(roles.Any() ? string.Join(", ", roles) : "Không có role nào")}
🔑 Is in Admin role: {isInAdminRole}

📋 All Available Roles:
{string.Join("\n", allRoles.Select(r => $"- {r.Name} (ID: {r.Id})"))}

🔗 Thử truy cập: /Admin
";
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Lỗi: {ex.Message}";
            }

            return View("TestAdmin");
        }
    }
}
