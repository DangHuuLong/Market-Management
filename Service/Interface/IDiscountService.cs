using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Service.Interface
{
    public interface IDiscountService
    {
        public Task<IEnumerable<Discount>> GetAllDiscountsAsync();
        public Task<Discount> GetDiscountByIdAsync(Guid discountId);
        public Task<IEnumerable<Discount>> GetDiscountsByNameAsync(string discountName);
        public Task AddDiscountAsync(Discount discount);
        public Task UpdateDiscountAsync(Discount discount);
        public Task DeleteDiscountAsync(Guid discountId);
    }
}
