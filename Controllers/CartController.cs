using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PBL3_HK4.Entity;
using PBL3_HK4.Service.Interface;
using System.Security.Claims;

namespace PBL3_HK4.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartItemService _cartItemService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;

        public CartController(ICartItemService cartItemService, IShoppingCartService shoppingCartService, IProductService productService)
        {
            _cartItemService = cartItemService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
        }

        [Authorize(Roles = "Customer")]
        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid productId, int quantity)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));

                if (cart == null)
                {
                    var newCart = new ShoppingCart
                    {
                        UserID = new Guid(userId),
                        CartID = Guid.NewGuid()
                    };
                    await _shoppingCartService.AddShoppingCartAsync(newCart);
                    cart = newCart;
                }

                HttpContext.Session.SetString("CartID", cart.CartID.ToString());

                IEnumerable<CartItem> existingItems;
                try
                {
                    existingItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);
                }
                catch (KeyNotFoundException)
                {
                    existingItems = new List<CartItem>();
                }

                var existingItem = existingItems.FirstOrDefault(i => i.ProductID == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    await _cartItemService.UpdateCartItemAsync(existingItem);
                }
                else
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    var newItem = new CartItem
                    {
                        ItemID = Guid.NewGuid(),
                        CartID = cart.CartID,
                        ProductID = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    await _cartItemService.AddCartItemAsync(newItem);
                }

                int itemCount;

                try
                {
                    var updatedItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);

                    itemCount = updatedItems.Count();
                }
                catch (KeyNotFoundException)
                {
                    itemCount = 1;
                }

                HttpContext.Session.SetInt32("CartItemCount", itemCount);

                return Json(new
                {
                    success = true,
                    message = "Đã thêm vào giỏ hàng thành công",
                    cartItemCount = itemCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCartItem(Guid cartitemid)
        {
            try
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _cartItemService.DeleteCartItemAsync(cartitemid);

                var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userid));
                int itemCount = 0;

                try
                {
                    var updatedItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);

                    itemCount = updatedItems.Count();
                }
                catch (KeyNotFoundException)
                {
                    itemCount = 0;
                }

                HttpContext.Session.SetInt32("CartItemCount", itemCount);

                return Ok(new { success = true, cartItemCount = itemCount }); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task UpdateQuantityCartItem(Guid cartitemid, bool increase)
        {
            await _cartItemService.UpdateQuantityCartItemAsync(cartitemid, increase);
        }
    }
}