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

                // Lưu CartID vào session
                HttpContext.Session.SetString("CartID", cart.CartID.ToString());

                // Thử lấy các mục trong giỏ hàng, nếu có lỗi thì khởi tạo danh sách rỗng
                IEnumerable<CartItem> existingItems;
                try
                {
                    existingItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);
                }
                catch (KeyNotFoundException)
                {
                    // Nếu không có mục nào, khởi tạo danh sách rỗng
                    existingItems = new List<CartItem>();
                }

                // Tìm mục đã tồn tại (nếu có)
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

                // Cập nhật số lượng LOẠI sản phẩm (không phải tổng số lượng)
                int itemCount;

                try
                {
                    // Lấy danh sách mới sau khi thêm/cập nhật
                    var updatedItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);

                    // Đếm số lượng loại sản phẩm trong giỏ hàng
                    itemCount = updatedItems.Count();
                }
                catch (KeyNotFoundException)
                {
                    // Nếu vẫn không có mục nào (rất hiếm gặp), giả định là 1 sản phẩm
                    itemCount = 1;
                }

                // Cập nhật session với số lượng loại sản phẩm
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
        //public async Task<IEnumerable<CartItem>> GetCartItems()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userId));
        //    return cart.Items;
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCartItem(Guid cartitemid)
        {
            try
            {
                var userid = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _cartItemService.DeleteCartItemAsync(cartitemid);

                // Cập nhật session sau khi xóa sản phẩm
                var cart = await _shoppingCartService.GetShoppingCartByCustomerIdAsync(new Guid(userid));
                int itemCount = 0;

                try
                {
                    // Lấy danh sách mới sau khi xóa
                    var updatedItems = await _cartItemService.GetCartItemsByShoppingCartIdAsync(cart.CartID);

                    // Đếm số lượng loại sản phẩm trong giỏ hàng
                    itemCount = updatedItems.Count();
                }
                catch (KeyNotFoundException)
                {
                    // Nếu không có mục nào, đặt count = 0
                    itemCount = 0;
                }

                // Cập nhật session với số lượng loại sản phẩm mới
                HttpContext.Session.SetInt32("CartItemCount", itemCount);

                return Ok(new { success = true, cartItemCount = itemCount }); // Trả về số lượng mới
            }
            catch (Exception ex)
            {
                // Log lỗi nếu có
                return BadRequest(new { success = false, message = ex.Message }); // Trả về 400 Bad Request khi có lỗi
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task UpdateQuantityCartItem(Guid cartitemid, bool increase)
        {
            await _cartItemService.UpdateQuantityCartItemAsync(cartitemid, increase);
            // Không return RedirectToAction
        }
    }
}