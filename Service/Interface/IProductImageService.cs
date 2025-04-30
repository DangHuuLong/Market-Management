using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Service.Interface
{
    public interface IProductImageService
    {
        public Task<List<ProductImage>> GetAllImagesByProductId(Guid productid);
        public Task<List<ProductImage>> GetAllImages();
        public Task<ProductImage> SaveImageAsync(IFormFile imageFile, Guid productId);
        public Task DeleteImageAsync(Guid imageId);
    }
}
