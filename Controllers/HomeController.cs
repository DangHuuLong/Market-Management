using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PBL3_HK4.Entity;
using PBL3_HK4.Models;
using PBL3_HK4.Service;
using PBL3_HK4.Service.Interface;

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
        public HomeController(IReviewService reviewService,IProductImageService productImageService, IDiscountService discountService,ICartItemService cartItemService,ICatalogService catalogService,IShoppingCartService shoppingCartService,IProductService productService, ICustomerService customerService, IBillService billService)
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

        public async Task<IActionResult> CompleteOrder(string paymentMethod, Guid? discountId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var customer = await _customerService.GetCustomerByIdAsync(new Guid(userId));
            Bill bill = new Bill();
            bill.BillID = Guid.NewGuid();
            bill.UserID = new Guid(userId);
            bill.Date = DateTime.Now;
            bill.Address = customer.Address;
            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            cart.Items = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);

            // Lấy thông tin discount trước vòng lặp nếu có
            Discount discount = null;
            if (discountId.HasValue)
            {
                discount = await _discountService.GetDiscountByIdAsync(discountId.Value);
            }

            bool inventoryError = false;
            string productWithError = "";
            int stockQuantity = 0;

            foreach (var item in cart.Items)
            {
                var product = await _productService.GetProductByIdAsync(item.ProductID);
                if (product.StockQuantity < item.Quantity)
                {
                    // Lưu thông tin sản phẩm lỗi
                    productWithError = product.ProductName;
                    stockQuantity = product.StockQuantity;
                    inventoryError = true;
                    break;
                }

                BillDetail billDetail = new BillDetail();
                billDetail.BillDetailID = Guid.NewGuid();
                billDetail.BillID = bill.BillID;
                billDetail.ProductID = item.ProductID;
                billDetail.Quantity = item.Quantity;
                billDetail.Price = item.Price;

                // Áp dụng discount (nếu có) cho tất cả sản phẩm
                if (discount != null)
                {
                    billDetail.DiscountID = discount.DiscountID;
                    billDetail.Total = (double)(billDetail.Quantity * billDetail.Price * (1 - discount.DiscountRate/100));
                   
                } else
                {
                    billDetail.Total = (double)(billDetail.Quantity * billDetail.Price);
                }

                bill.BillDetails.Add(billDetail);
            }

            if (inventoryError)
            {
                TempData["ErrorMessage"] = $"Product {productWithError} only has {stockQuantity} items in stock!";
                TempData.Keep("ErrorMessage");
                return RedirectToAction("OrderSummary");
            }

            var itemsToDelete = cart.Items.Select(item => item.ItemID).ToList();
            foreach (var itemId in itemsToDelete)
            {
                await _cartItemService.DeleteCartItemAsync(itemId);
            }

            await _billService.AddBillAsync(bill);
            HttpContext.Session.SetInt32("CartItemCount", 0);
            TempData["SuccessMessage"] = "Your order has been placed successfully!";
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
    }
}
