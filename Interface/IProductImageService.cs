using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Interface
{
    public interface IProductImageService
    {
        public Task<ProductImage> SaveImageAsync(IFormFile imageFile, Guid productId);
        public Task DeleteImageAsync(Guid imageId);
    }
}
