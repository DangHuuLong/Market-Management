using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PBL3_HK4.Entity;
using PBL3_HK4.Interface;
using PBL3_HK4.Models;
using PBL3_HK4.Service;
using PBL3_HK4.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PBL3_HK4.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICatalogService _catalogService;
        private readonly IBillService _billService;
        private readonly ICartItemService _cartItemService;
        private readonly IDiscountService _discountService;
        private readonly IProductImageService _productImageService;
        private readonly IReviewService _reviewService;
        public readonly IBillDetailService _billDetailService;
        private readonly IVnPayService _vnPayService;
        public HomeController(IBillDetailService billDetailService, IReviewService reviewService, IProductImageService productImageService, IDiscountService discountService, ICartItemService cartItemService, ICatalogService catalogService, IShoppingCartService shoppingCartService, IProductService productService, ICustomerService customerService, IBillService billService, IVnPayService vnPayService)
        {
            _reviewService = reviewService;
            _productImageService = productImageService;
            _cartItemService = cartItemService;
            _billService = billService;
            _customerService = customerService;
            _catalogService = catalogService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _customerService = customerService;
            _discountService = discountService;
            _billDetailService = billDetailService;
            _vnPayService = vnPayService;
        }

        [Route("Home/Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            foreach (var product in products)
            {
                var reviews = await _reviewService.GetReviewsByProductIdAsync(product.ProductID);
                product.Reviews = reviews.ToList();
            }

            var homeViewModel = new HomeViewModel
            {
                Products = products,
                Catalogs = await _catalogService.GetAllCatalogsAsync(),
                ProductImages = await _productImageService.GetAllImages()
            };
            homeViewModel.AssignImagesToProducts();
            return View("Index", homeViewModel);
        }


        public async Task<IActionResult> ShoppingCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            var products = await _productService.GetAllProductsAsync();
            var productImages = await _productImageService.GetAllImages();
            var cartItem = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);
            cart.Items = cartItem;
            foreach (var product in products)
            {
                var images = productImages
                    .Where(image => image.ProductID == product.ProductID)
                    .ToList();
                product.Images = images;
            }
            var viewmodel = new ShoppingCartViewModel
            {
                ShoppingCart = cart,
                Products = products
            };

            return View(viewmodel);
        }

        public async Task<IActionResult> OrderSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));


            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            var products = await _productService.GetAllProductsAsync();
            var cartItem = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);
            var discounts = await _discountService.GetAllDiscountsAsync();
            cart.Items = cartItem;
            OrderSummaryViewModel orderSummaryViewModel = new OrderSummaryViewModel
            {
                ShoppingCart = cart,
                Products = products,
                Customer = customer,
                Discounts = discounts
            };


            return View(orderSummaryViewModel);
        }
        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> DeductPoints(Guid customerId)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(customerId);
                if (customer == null)
                {
                    return Json(new { success = false, message = "Customer not found." });
                }
                const int pointsToDeduct = 2;
                if (customer.EarnedPoint < pointsToDeduct)
                {
                    return Json(new { success = false, message = "Not enough points to spin! You need at least 2 points." });
                }
                customer.EarnedPoint -= pointsToDeduct;
                await _customerService.UpdateCustomerAsync(customer);
                return Json(new
                {
                    success = true,
                    message = "Points deducted successfully.",
                    remainingPoints = customer.EarnedPoint
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        public async Task<IActionResult> Notification()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));
            var bills = await _billService.GetBillByUserIdAsync(new Guid(userId));
            var products = await _productService.GetAllProductsAsync();

            CustomerOrderModelView customerOrderModelView = new CustomerOrderModelView
            {
                Customer = customer,
                Bills = bills,
                Products = products
            };

            return View(customerOrderModelView);
        }

        public IActionResult CompleteOrder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompleteOrder(Guid? discountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            cart.Items = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);

            // Kiểm tra tồn kho
            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductID);
                if (product.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm {product.ProductName} chỉ còn {product.StockQuantity} trong kho!";
                    return RedirectToAction("OrderSummary");
                }
            }

            // Tính tổng tiền và lưu cart vào session
            double totalAmount = 0;
            var tempItems = new List<TempCartItem>();
            foreach (var item in cart.Items)
            {
                var discountRate = 0.0;
                if (discountId.HasValue)
                {
                    var discount = await _discountService.GetDiscountByIdAsync(discountId.Value);
                    discountRate = discount?.DiscountRate ?? 0;
                }

                var itemTotal = item.Quantity * item.Price * (1 - discountRate / 100.0);
                totalAmount += itemTotal;

                tempItems.Add(new TempCartItem
                {
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price
                });
            }

            var tempData = new TempBillData
            {
                DiscountId = discountId,
                CartItems = tempItems,
                TotalAmount = totalAmount
            };

            HttpContext.Session.SetString("PendingOrder", JsonConvert.SerializeObject(tempData));
            return RedirectToAction("ProcessPayment", new { amount = totalAmount });
        }



        [HttpGet]
        public async Task<IActionResult> ProcessPayment(double amount)
        {
            // Lấy user và thông tin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));

            // Tạo PaymentInformationModel
            var paymentModel = new PaymentInformationModel
            {
                Name = customer.Name,
                OrderDescription = "Thanh toán đơn hàng",
                Amount = amount,
                OrderType = "bill"
            };

            // Tạo URL thanh toán
            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);

            // Chuyển hướng sang trang VNPAY
            return Redirect(paymentUrl);
        }
        [HttpGet]
        public async Task<IActionResult> VnPayReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (!response.Success)
            {
                TempData["ErrorMessage"] = "Thanh toán thất bại hoặc bị hủy!";
                return RedirectToAction("OrderSummary");
            }

            // Lấy dữ liệu từ Session
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));
            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            cart.Items = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);

            var jsonData = HttpContext.Session.GetString("PendingOrder");
            if (string.IsNullOrEmpty(jsonData))
            {
                TempData["ErrorMessage"] = "Không tìm thấy dữ liệu đơn hàng trong session.";
                return RedirectToAction("OrderSummary");
            }

            var orderData = JsonConvert.DeserializeObject<TempBillData>(jsonData);

            // Tạo Bill
            var bill = new Bill
            {
                BillID = Guid.NewGuid(),
                UserID = new Guid(userId),
                Date = DateTime.Now,
                Address = customer.Address,
                BillDetails = new List<BillDetail>()
            };

            foreach (var item in orderData.CartItems)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductID);
                if (product.StockQuantity < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Sản phẩm {product.ProductName} không đủ tồn kho!";
                    return RedirectToAction("OrderSummary");
                }

                // Trừ kho
                await _productService.DecreaseStock(item.ProductID, item.Quantity);

                var billDetail = new BillDetail
                {
                    BillDetailID = Guid.NewGuid(),
                    BillID = bill.BillID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Quantity * item.Price,
                    DiscountID = orderData.DiscountId
                };

                bill.BillDetails.Add(billDetail);
            }


            await _billService.AddBillAsync(bill);


            var itemsToDelete = cart.Items.Select(x => x.ItemID).ToList();
            foreach (var id in itemsToDelete)
            {
                await _cartItemService.DeleteCartItemAsync(id);
            }

            HttpContext.Session.SetInt32("CartItemCount", 0);
            HttpContext.Session.Remove("PendingOrder");

            TempData["SuccessMessage"] = "Thanh toán thành công! Đơn hàng đã được tạo.";
            return RedirectToAction("CompleteOrder");
        }


    }
}
