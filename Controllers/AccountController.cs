using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBL3_HK4.Entity;
using PBL3_HK4.Interface;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using PBL3_HK4.Service;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Controllers
{
    public class AccountController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IAdminService _adminService;
        private readonly IAccountService _accountService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICartItemService _cartItemService;
        private readonly IPasswordHasher _passwordHasher;

        public AccountController(ICartItemService cartItemService, ICustomerService customerService, IAdminService adminService, IAccountService accountService, IShoppingCartService shoppingCartService, IPasswordHasher passwordHasher)
        {
            _cartItemService = cartItemService;
            _customerService = customerService;
            _accountService = accountService;
            _adminService = adminService;
            _shoppingCartService = shoppingCartService;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Main()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(Customer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            var customers = await _customerService.GetAllCustomerAsync();
            foreach (Customer cus in customers)
            {
                if (cus.Email == customer.Email)
                {
                    ModelState.AddModelError("Email", "This email is already in use. Please choose a different email.");
                    return View(customer);
                }
            }

            try
            {
                await _accountService.RegisterAsync(customer.Name, customer.Email, customer.Sex, customer.DateOfBirth,
                    customer.UserName, customer.Phone, customer.PassWord, customer.Address);

                return RedirectToAction("SignIn", "Account");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(customer);
            }
        }
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var userLogin = await _accountService.LoginAsync(user.UserName, user.PassWord);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userLogin.UserName),
                    new Claim(ClaimTypes.NameIdentifier, userLogin.UserID.ToString()),
                    new Claim(ClaimTypes.Role, userLogin.Role),
                    new Claim(ClaimTypes.Email, userLogin.Email)
                };
                var role = User.FindFirstValue(ClaimTypes.Role);
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );
                if (userLogin is Customer customer)
                {
                    var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(customer.UserID);

                    HttpContext.Session.SetString("CartID", cart.CartID.ToString());

                    try
                    {
                        var cartItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);
                        int itemCount = cartItems.Count();
                        HttpContext.Session.SetInt32("CartItemCount", itemCount);
                    }
                    catch (KeyNotFoundException)
                    {
                        HttpContext.Session.SetInt32("CartItemCount", 0);
                    }

                    return RedirectToAction("Index", "Home"); 
                }
                else
                {
                    return RedirectToAction("Home", "Admin"); 
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(user);
            }
        }

        public async Task<IActionResult> SignOut()
        {
            HttpContext.Session.Remove("CartID");
            HttpContext.Session.Remove("CartItemCount");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await _accountService.Logout();
            return RedirectToAction("Main", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                if (user.NewPassWord.IsNullOrEmpty())
                {
                    ModelState.AddModelError(string.Empty, "New password cannot be empty.");
                    return View(user);
                }
                await _accountService.ChangePasswordAsync(user.UserName, user.PassWord, user.NewPassWord);
                return RedirectToAction("Index", "Home");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        public IActionResult SendCode()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendCode(string email)
        {
            var defiCode = await _accountService.GenerateVerificationCode();
            await _accountService.SendPasswordResetEmailAsync(email, defiCode);
            return View("ForgotPassword", email);
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string code, string newPassword, string email)
        {
            var user = await _customerService.GetUserByEmailAsync(email);
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out Guid id);

            if (code == user.VerificationCode && DateTime.UtcNow < user.VerificationCodeExpiry)
            {
                Customer newUser = new Customer()
                {
                    UserID = user.UserID,
                    PassWord = _passwordHasher.HashPassword(newPassword)
                };
                await _customerService.UpdateCustomerAsync(newUser);
                return View("SignIn");
            }
            return View("ForgotPassword", email);
        }

    }
}