using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PBL3_HK4.Entity;

namespace PBL3_HK4.Service.Interface
{
    public interface IProductService
    {
        public Task<IEnumerable<Product>> GetAllProductsAsync();
        public Task<Product> GetProductByIdAsync(Guid productId);
        public Task<IEnumerable<Product>> GetProductsByNameAsync(string productName);
        public Task<IEnumerable<Product>> GetProductsByCatalogIdAsync(Guid catalogId);
        public Task AddProductAsync(Product product);
        public Task UpdateProductAsync(Product product);
        public Task DeleteProductAsync(Guid productId);

    }
}
