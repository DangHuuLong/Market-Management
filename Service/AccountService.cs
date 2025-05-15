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
using PBL3_HK4.Interface;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Web;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Net;
using PBL3_HK4.Service.Interface;
namespace PBL3_HK4.Service
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ICustomerService _customerService;
        private readonly IAdminService _adminService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AccountService(ApplicationDbContext context, ICustomerService customerService, IAdminService adminService, IPasswordHasher passwordHasher, IShoppingCartService shoppingCartService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _customerService = customerService;
            _adminService = adminService;
            _passwordHasher = passwordHasher;
            _shoppingCartService = shoppingCartService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
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
            if (user is Customer)
            {
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


        public async Task RegisterAsync(string name, string email, string sex, DateTime dateofbirth, string username, string phone,
     string password, string address)
        {
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


        public async Task<bool> SendPasswordResetEmailAsync(string email, string code)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                Console.WriteLine("Email address is null or empty.");
                return false;
            }

            try
            {
                Console.WriteLine($"Attempting to send verification code to: {email}");

                var user = await _customerService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    Console.WriteLine($"No user found with email: {email}");
                    return false;
                }

                var verificationCode = code;
                var codeExpiry = DateTime.UtcNow.AddMinutes(15);

                user.VerificationCode = verificationCode;
                user.VerificationCodeExpiry = codeExpiry;
                await _customerService.UpdateUserAsync(user);

                var message = new MimeMessage();

                var senderEmail = _configuration["Smtp:SenderEmail"]?.Trim();
                if (string.IsNullOrEmpty(senderEmail))
                {
                    Console.WriteLine("Sender email is not configured.");
                    return false;
                }
                message.From.Add(new MailboxAddress("PBL3_HK4 Verification", senderEmail));

                message.To.Add(new MailboxAddress(user.UserName ?? "User", email));

                message.Subject = "Your Verification Code";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                <p>Hello {user.UserName ?? "User"},</p>
                <p>Your verification code is: <strong style='font-size: 18px;'>{verificationCode}</strong></p>
                <p>Please enter this code in the verification page to proceed with password reset.</p>
                <p>This code will expire at {codeExpiry:HH:mm} UTC.</p>
                <p>If you didn't request this, please ignore this email or contact support.</p>
                <p>Thank you,</p>
                <p>The PBL3_HK4 Team</p>"
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                try
                {
                    var smtpHost = _configuration["Smtp:Host"]?.Trim();
                    var smtpPort = _configuration["Smtp:Port"]?.Trim();

                    Console.WriteLine($"Connecting to SMTP server at {smtpHost}:{smtpPort}");
                    await client.ConnectAsync(smtpHost, int.Parse(smtpPort), SecureSocketOptions.StartTls);

                    var smtpUsername = _configuration["Smtp:Username"]?.Trim();
                    var smtpPassword = _configuration["Smtp:Password"]?.Trim();

                    if (string.IsNullOrEmpty(smtpUsername)) throw new ArgumentNullException("SMTP Username is not configured");
                    if (string.IsNullOrEmpty(smtpPassword)) throw new ArgumentNullException("SMTP Password is not configured");

                    Console.WriteLine("Authenticating with SMTP server...");
                    await client.AuthenticateAsync(smtpUsername, smtpPassword);

                    Console.WriteLine("Sending verification code email...");
                    await client.SendAsync(message);

                    Console.WriteLine("Verification code email sent successfully.");
                    return true;
                }
                finally
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync(true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending verification email: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<string> GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}