using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PBL3_HK4.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using PBL3_HK4.Models;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICatalogService _catalogService;
        private readonly IProductImageService _productImageService;
        private readonly IReviewService _reviewService; 
        private readonly ICustomerService _customerService;
        public ProductController(ICustomerService customerService,IReviewService reviewService,ICatalogService catalogService, IProductService productService, IProductImageService productImageService)
        {
            _customerService = customerService;
            _reviewService = reviewService;
            _catalogService = catalogService;
            _productService = productService;
            _productImageService = productImageService;
        }

        [Route("Product/Details")]
        public async Task<IActionResult> Details(Guid id)
        {
            var customers = await _customerService.GetAllCustomerAsync();
            var listProduct = await _productService.GetAllProductsAsync();
            var product = await _productService.GetProductByIdAsync(id);
            var catalog = await _catalogService.GetCatalogByIdAsync(product.CatalogID);
            var productImages = await _productImageService.GetAllImagesByProductId(id);
            var images = productImages.Where(image => image.ProductID == product.ProductID).ToList();
            product.Images = images;
            product.Catalog = catalog;
            product.Reviews = (await _reviewService.GetReviewsByProductIdAsync(id)).ToList();

            // Lấy tất cả ảnh cho các sản phẩm trong listProduct
            var allProductImages = await _productImageService.GetAllImages();
            foreach (var prod in listProduct)
            {
                var prodImages = allProductImages
                    .Where(image => image.ProductID == prod.ProductID)
                    .ToList();
                prod.Images = prodImages;

                var reviews = await _reviewService.GetReviewsByProductIdAsync(prod.ProductID);
                prod.Reviews = reviews.ToList();
            }

            var viewModel = new ProductDetailsViewModel
            {
                Customer = customers,
                Product = product,
                ListProduct = listProduct
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(List<IFormFile> imgfile, Product product)
        {
            if (imgfile == null || product == null)
            {
                return View(); // show ra cai j
            }
            await _productService.AddProductAsync(product);
            foreach (var img in imgfile)
            {
                await _productImageService.SaveImageAsync(img, product.ProductID);
            }
            return RedirectToAction("Index"); // show ra full san pham
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }


        public IActionResult Views()
        {
            return View();
        }
    }
}
