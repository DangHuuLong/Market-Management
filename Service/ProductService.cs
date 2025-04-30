using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;
using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Service.Interface;

namespace PBL3_HK4.Service
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }



        public  async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            var listProduct = await _context.Products.ToListAsync();
            if (listProduct == null) 
            {
                throw new KeyNotFoundException("No products found.");
            }
            return listProduct;
        }

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            var product = await _context.Products.Where(p => p.ProductID == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID:{productId} not found");
            }
            return product;
        }

        public async Task<IEnumerable<Product>> GetProductsByNameAsync(string productName)
        {
            var listProduct = await _context.Products.Where(p => p.ProductName == productName).ToListAsync();
            if (listProduct.Count == 0)
            {
                throw new KeyNotFoundException($"Products with name {productName}");
            }
            return listProduct;
        }

        public async Task<IEnumerable<Product>> GetProductsByCatalogIdAsync(Guid catalogId)
        {
            var listProduct = await _context.Products.Where(p => p.CatalogID == catalogId).ToListAsync();
            if (listProduct == null || listProduct.Count == 0)
            {
                throw new KeyNotFoundException($"Products with catalog ID:{catalogId} not found");
            }
            return listProduct;
        }

        public async Task AddProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with ID:{product.ProductID} already exists");
            }
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        //public async Task UpdateProductAsync(Product product)
        //{
        //    var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
        //    if (existingProduct == null)
        //    {
        //        throw new KeyNotFoundException($"Product with ID:{product.ProductID} not found");
        //    }
        //    _context.Products.Update(product);
        //    await _context.SaveChangesAsync();
        //}

        public async Task UpdateProductAsync(Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException($"Product with ID:{product.ProductID} not found");
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.ProductDescription = product.ProductDescription;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.CatalogID = product.CatalogID;
            existingProduct.MFGDate = product.MFGDate;
            existingProduct.EXPDate = product.EXPDate;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            var product = await _context.Products.Where(p => p.ProductID == productId).FirstOrDefaultAsync();
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID:{productId} not found");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
