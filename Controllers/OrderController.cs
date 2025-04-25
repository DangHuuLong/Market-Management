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
using System;
using PBL3_HK4.Service;

namespace PBL3_HK4.Controllers
{
    public class OrderController : Controller
    {
        private readonly IBillService _billService;
        private readonly IProductService _productService;
        private readonly IBillDetailService _billDetailService;
        private readonly ICustomerService _customerService;
        private readonly IReviewService _reviewService;
        private readonly IProductImageService _productImageService;

        public OrderController(IProductImageService productImageService,IReviewService reviewService,IBillService billService, IProductService productService, IBillDetailService billDetailService, ICustomerService customerService)
        {
            _productImageService = productImageService;
            _reviewService = reviewService;
            _billDetailService = billDetailService;
            _customerService = customerService;
            _billService = billService;
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(Customer customer, IEnumerable<CartItem> items, Discount? discount)
        {
            if (items == null || !items.Any())
            {
                return Json(new { success = false, message = "No items in the cart." });
            }

            List<BillDetail> billDetails = new List<BillDetail>();
            var billid = Guid.NewGuid();

            foreach (var item in items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductID);
                if (product == null)
                {
                    return Json(new { success = false, message = $"Product with ID {item.ProductID} not found." });
                }
                if (item.Quantity > product.StockQuantity)
                {
                    return Json(new { success = false, message = $"Not enough stock for product {product.ProductName}." });
                }
                billDetails.Add(new BillDetail
                {
                    BillDetailID = item.ItemID,
                    Quantity = item.Quantity,
                    ProductID = product.ProductID,
                    BillID = billid,
                    DiscountID = discount?.DiscountID,
                    Price = product.Price,
                });
            }

            Bill bill = new Bill
            {
                BillID = billid,
                UserID = customer.UserID,
                Address = customer.Address,
                Date = DateTime.Now,
                BillDetails = billDetails,
                Status = BillStatus.Unconfirmed // Assuming initial status
            };

            await _billService.AddBillAsync(bill);
            return Json(new { success = true, message = "Order confirmed successfully." });
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
                BillDetails = billDetails,
                Products = products
            };

            return View(customerOrderModelView);
        }

        [HttpPost]
        public async Task<IActionResult> CancelOrder(Guid billId, string reason)
        {
            var bill = await _billService.GetBillByIdAsync(billId);
            if (bill == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }
            if (bill.Status != BillStatus.Unconfirmed && bill.Status != BillStatus.Confirmed)
            {
                return Json(new { success = false, message = "Order cannot be canceled in its current status." });
            }

            await _billService.UpdateBillCanceledAsync(billId);
            return Json(new { success = true, message = "Order canceled successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveOrder(Guid billId)
        {
            var bill = await _billService.GetBillByIdAsync(billId);
            if (bill == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }
            if (bill.Status != BillStatus.Confirmed)
            {
                return Json(new { success = false, message = "Order is not in Confirmed status." });
            }

            await _billService.UpdateBillReceivedAsync(billId);
            return Json(new { success = true, message = "Order marked as received successfully." });
        }

        public async Task<IActionResult> Reviews(Guid billId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));

            var bill = await _billService.GetBillByIdAsync(billId);
            var billDetails = await _billDetailService.GetAllBillDetailsAsync();

            var products = new List<Product>();

            foreach (var billDetail in billDetails)
            {
                if (billDetail.BillID == billId)
                {
                    var product = await _productService.GetProductByIdAsync(billDetail.ProductID);
                    if (product != null) 
                    {
                        products.Add(product); 
                    }
                }
            }

            var allProductImages = await _productImageService.GetAllImages();
            foreach (var prod in products)
            {
                var prodImages = allProductImages
                    .Where(image => image.ProductID == prod.ProductID)
                    .ToList();
                prod.Images = prodImages;
            }

            ReviewsViewModel reviewsViewModel = new ReviewsViewModel
            {
                Customer = customer,
                Products = products,
            };

            return View(reviewsViewModel); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReviews(List<Review> Reviews)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Dữ liệu đánh giá không hợp lệ.");
            }

            try
            {
                foreach (var review in Reviews)
                {
                    review.ReviewID = Guid.NewGuid();
                    review.CreateAt = DateTime.Now;
                    await _reviewService.AddReviewAsync(review);
                }
                return View();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Lỗi khi lưu đánh giá.");
            }
        }
    }
}