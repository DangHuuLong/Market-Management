using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Interface;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
namespace PBL3_HK4.Service
{
    public class ProductImageService : IProductImageService
    {
        private readonly string _imagesFolder;
        private readonly ApplicationDbContext _context;


        public ProductImageService(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _imagesFolder = Path.Combine(environment.WebRootPath, "images", "products");

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<ProductImage> SaveImageAsync(IFormFile imageFile, Guid productId)
        {
            // 1. Tạo tên file duy nhất
            var imageId = Guid.NewGuid();
            var fileName = $"{productId}-{imageId}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(_imagesFolder, fileName);

            // 2. Lưu file vào wwwroot
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // 3. Tạo đối tượng ProductImage
            var productImage = new ProductImage
            {
                ImageID = imageId,
                ProductID = productId,
                ImagePath = $"/images/products/{fileName}"
            };

            // 4. Lưu vào database
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return productImage;
        }

        public async Task DeleteImageAsync(Guid imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
                return;

            // 1. Xóa file vật lý
            var fileName = Path.GetFileName(image.ImagePath);
            var filePath = Path.Combine(_imagesFolder, fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            // 2. Xóa bản ghi database
            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }
}
