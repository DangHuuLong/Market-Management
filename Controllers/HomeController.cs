using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PBL3_HK4.Entity;
using PBL3_HK4.Interface;
using PBL3_HK4.Models;
using PBL3_HK4.Service;

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
        public HomeController(ICartItemService cartItemService,ICatalogService catalogService,IShoppingCartService shoppingCartService,IProductService productService, ICustomerService customerService, IBillService billService)
        {
            _cartItemService = cartItemService;
            _billService = billService; 
            _customerService = customerService;
            _catalogService = catalogService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _customerService = customerService;
        }

        [Route("Home/Index")]
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var catalogs = await _catalogService.GetAllCatalogsAsync();
            var homeViewModel = new HomeViewModel
            {
                Products = products,
                Catalogs = catalogs,

            };
            return View("Index", homeViewModel);
        }
        

        public async Task<IActionResult> ShoppingCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
            var products = await _productService.GetAllProductsAsync();
            var cartItem = await _shoppingCartService.GetCartItemsByCartIDAsync(cart.CartID);
            cart.Items = cartItem;
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
            cart.Items = cartItem;
            OrderSummaryViewModel orderSummaryViewModel = new OrderSummaryViewModel
            {
                ShoppingCart = cart,
                Products = products,
                Customer = customer
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

        public async Task<IActionResult> CompleteOrder()
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
    }
}
