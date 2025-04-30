using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Service
{
    public class ProductImageService : IProductImageService
    {
        private readonly string _imagesFolder;
        private readonly ApplicationDbContext _context;

        public ProductImageService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _imagesFolder = Path.Combine(environment.WebRootPath, "images", "products");

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);
        }

        public async Task<List<ProductImage>> GetAllImagesByProductId(Guid productid)
        {
            var list = await _context.ProductImages.Where(pi => pi.ProductID == productid).ToListAsync();
            if (list == null)
            {
                throw new Exception("There's no images");
            }
            return list;
        }

        public async Task<List<ProductImage>> GetAllImages()
        {
            var list = await _context.ProductImages.ToListAsync();
            if (list == null)
            {
                throw new Exception("There's no images");
            }
            return list;
        }

        public async Task<ProductImage> SaveImageAsync(IFormFile imageFile, Guid productId)
        {
            var imageId = Guid.NewGuid();
            var fileName = $"{productId}-{imageId}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(_imagesFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var productImage = new ProductImage
            {
                ImageID = imageId,
                ProductID = productId,
                ImagePath = $"/images/products/{fileName}"
            };

            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            return productImage;
        }

        public async Task DeleteImageAsync(Guid imageId)
        {
            var image = await _context.ProductImages.FindAsync(imageId);
            if (image == null)
            {
                return;
            }

            var fileName = Path.GetFileName(image.ImagePath);
            var filePath = Path.Combine(_imagesFolder, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            else
            {
                Console.WriteLine($"File not found at path: {filePath}");
            }

            _context.ProductImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }
}