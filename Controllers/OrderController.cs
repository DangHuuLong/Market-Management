using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBL3_HK4.Entity;
using PBL3_HK4.Interface;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using PBL3_HK4.Models;

namespace PBL3_HK4.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBillService _billService;
        private readonly IProductService _productService;
        private readonly IBillDetailService _billDetailService;
        private readonly ICustomerService _customerService;
        public OrderController(IBillService billService, IProductService productService, IBillDetailService billDetailService, ICustomerService customerService)
        {
            _billDetailService = billDetailService;
            _customerService = customerService;
            _billService = billService;
            _productService = productService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<bool> ConfirmOrder(Customer customer, IEnumerable<CartItem> items, Discount? discount)
        {
            if (items == null)
            {
                return false;
            }
            List<BillDetail> billDetails = new List<BillDetail>();
            var billid = Guid.NewGuid();
            foreach (var item in items)
            {
                var itemid = item.ProductID;
                int quantity = item.Quantity;
                var product = await _productService.GetProductByIdAsync(itemid);
                if (quantity > product.StockQuantity)
                    return false;
                else
                {
                    billDetails.Add(new BillDetail
                    {
                        BillDetailID = item.ItemID,
                        Quantity = quantity,
                        ProductID = product.ProductID,
                        BillID = billid,
                        DiscountID = discount?.DiscountID,
                        Price = product.Price,
                    });
                }
            }
            Bill bill = new Bill
            {
                BillID = billid,
                UserID = customer.UserID,
                Address = customer.Address,
                Date = DateTime.Now,
                BillDetails = billDetails
            };
            await _billService.AddBillAsync(bill);
            return true;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));
            var bills = await _billService.GetBillByUserIdAsync(new Guid(userId));
            var billDetails = new List<BillDetail>();
            var products = await _productService.GetAllProductsAsync();

            foreach (var bill in bills)
            {
                var billDetail = await _billDetailService.GetBillDetailsByBillIdAsync(bill.BillID);
                billDetails.AddRange(billDetail);
            }

            CustomerOrderModelView customerOrderModelView = new CustomerOrderModelView
            {
                Customer = customer,
                Bills = bills,
                BillDetails= billDetails,
                Products = products
            };

            return View(customerOrderModelView);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid billId, string reason)
        {
            await _billService.UpdateBillCanceledAsync(billId);
            return RedirectToAction("Index", "Order");
        }
    }
}