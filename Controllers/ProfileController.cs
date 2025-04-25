using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBL3_HK4.Entity;
using PBL3_HK4.Interface;
using System.Security.Claims;
using System.Threading.Tasks;
namespace PBL3_HK4.Controllers
{
    public class ProfileController : Controller
    {

        private readonly ICustomerService _customerService;
        private readonly IAdminService _adminService;
        private readonly IPasswordHasher _passwordHasher;
        public ProfileController(ICustomerService customerService, IAdminService adminService, IPasswordHasher passwordHasher)
        {
            _customerService = customerService;
            _adminService = adminService;
            _passwordHasher = passwordHasher;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("/Profile/Index")]
        public async Task<IActionResult> ShowProfile()
        {
            var username = User.Identity.Name;
            var role = User.FindFirstValue(ClaimTypes.Role);
            User userProfile;
            if (role == "Customer")
            {
                userProfile = await _customerService.GetCustomerByUserNameAsync(username);
                userProfile = (Customer)userProfile;
            }
            else
            {
                userProfile = await _adminService.GetAdminByUserNameAsync(username);
                userProfile = (Admin)userProfile;
            }
            return View("Index", userProfile);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Customer customer)
        {
            if (customer.NewPassWord != null)
            {
                var newpass = customer.NewPassWord;
                var password = _passwordHasher.HashPassword(newpass);
                customer.PassWord = password;
                customer.NewPassWord = null;
            }
            await _customerService.UpdateCustomerAsync(customer);
            return RedirectToAction("SignIn","Account");
        }
    }
}