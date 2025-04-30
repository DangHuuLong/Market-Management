using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Service.Interface;
namespace PBL3_HK4.Service
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICustomerService _customerService;
        private readonly IAdminService _adminService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(ApplicationDbContext context, ICustomerService customerService, IAdminService adminService, IPasswordHasher passwordHasher, IShoppingCartService shoppingCartService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _customerService = customerService;
            _adminService = adminService;
            _passwordHasher = passwordHasher;
            _shoppingCartService = shoppingCartService;
            _httpContextAccessor = httpContextAccessor;

        }
        public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (user == null)
            {
                throw new InvalidOperationException($"User with username {username} not found.");
            }

            bool isValidOldPassword = _passwordHasher.VerifyPassword(user.PassWord, oldPassword);
            if (!isValidOldPassword)
            {
                throw new InvalidOperationException("Current password is incorrect.");
            }
            user.PassWord = _passwordHasher.HashPassword(newPassword);
            if(user is Customer) {
                await _customerService.UpdateCustomerAsync((Customer)user);
                return true;
            }
            else
            {
                await _adminService.UpdateAdminAsync((Admin)user);
                return true;
            }
            
        }

        public async Task<User> LoginAsync(string username, string password)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (existingUser == null)
            {
                throw new InvalidOperationException($"Invalid username or password.");
            }
            var hashedPassword = existingUser.PassWord;
            bool isValid = _passwordHasher.VerifyPassword(hashedPassword, password);
            if (!isValid)
            {
                throw new InvalidOperationException($"Invalid username or password.");
            }
            return existingUser;
        }

        public async Task Logout()
        {
            if (_httpContextAccessor?.HttpContext != null)
            {

                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                _httpContextAccessor.HttpContext.Session.Clear();
            }
        }


        public async Task RegisterAsync(string name, string email, string sex, DateTime dateofbirth, string username,string phone,
     string password, string address)
        {
            // Kiểm tra username tồn tại
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserName == username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with username {username} already exists.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var userId = Guid.NewGuid();
                var customer = new Customer
                {
                    UserID = userId,
                    Name = name,
                    Email = email,
                    Sex = sex,
                    DateOfBirth = dateofbirth,
                    UserName = username,
                    PassWord = _passwordHasher.HashPassword(password),
                    Address = address,
                    EarnedPoint = 0,
                    Role = "Customer",
                    Phone = phone
                };
               await _customerService.AddCustomerAsync(customer);

                var shoppingCart = new ShoppingCart
                {
                    CartID = Guid.NewGuid(),
                    UserID = userId
                };

                await _shoppingCartService.AddShoppingCartAsync(shoppingCart);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception("Registration failed. Please try again.", ex);
            }
        }
    }
}
