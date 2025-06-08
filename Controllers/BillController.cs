using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBL3_HK4.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using PBL3_HK4.Service;
using Microsoft.AspNetCore.Authorization;
using PBL3_HK4.Service.Interface;
namespace PBL3_HK4.Controllers
{
    public class BillController : Controller
    {
        private readonly IBillService _billService;
        private readonly IAdminService _adminService;

        public BillController(IBillService billService, IAdminService adminService)
        {
            _billService = billService;
            _adminService = adminService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Views()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [Route("/Bill")]
        public async Task<IActionResult> UpdateBillAsync(Bill bill)
        {
            if (!ModelState.IsValid)
                return View(); 
            try
            {
                await _billService.UpdateBillAsync(bill);

                return RedirectToAction("Index", "Home"); 
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(); 
            }
        }

        [Authorize(Roles = "Admin")]
        [Route("/Bill/{id}")]
        public async Task<IActionResult> DeleteBillAsync(Guid id)
        {
            if (!ModelState.IsValid)
                return View(); 
            await _billService.DeleteBillAsync(id);

            return RedirectToAction("Index", "Home");
        }

    }
}
