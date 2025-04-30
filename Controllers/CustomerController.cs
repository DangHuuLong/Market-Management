using global::PBL3_HK4.Entity;
using Microsoft.AspNetCore.Mvc;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _userService;
        private readonly IAccountService _accountService;

        public CustomerController(ICustomerService userService, IAccountService accountService)
        {
            _userService = userService;
            _accountService = accountService; ;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _accountService.RegisterAsync(customer.Name, customer.Email, customer.Sex, customer.DateOfBirth, customer.UserName, customer.Phone,
                customer.PassWord, customer.Address);

            return Ok(new { message = "Đăng ký thành công!" });
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Views()
        {
            return View();
        }
    }

}
