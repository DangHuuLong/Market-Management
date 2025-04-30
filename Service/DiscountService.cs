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
    public class DiscountService: IDiscountService
    {
        private readonly ApplicationDbContext _context;

        public DiscountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Discount>> GetAllDiscountsAsync()
        {
            var listDiscount = await _context.Discounts.ToListAsync();
            if (listDiscount == null)
            {
                throw new KeyNotFoundException("No discounts found.");
            }
            return listDiscount;
        }

        public async Task<Discount> GetDiscountByIdAsync(Guid discountId)
        {
            var discount = await _context.Discounts.Where(d => d.DiscountID == discountId).FirstOrDefaultAsync();
            if (discount == null)
            {
                throw new KeyNotFoundException($"Discount with ID:{discountId} not found");
            }
            return discount;
        }

        public async Task<IEnumerable<Discount>> GetDiscountsByNameAsync(string discountName)
        {
            List<Discount> listDiscount = await _context.Discounts.Where(d => d.Name == discountName).ToListAsync();
            if (listDiscount.Count == 0)
            {
                throw new KeyNotFoundException($"Discounts with name {discountName} not found");
            }
            return listDiscount;
        }

        public async Task AddDiscountAsync(Discount discount)
        {
            var existingDiscount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountID == discount.DiscountID);
            if (existingDiscount != null)
            {
                throw new InvalidOperationException($"Discount with ID {discount.DiscountID} already exists.");
            }
            await _context.Discounts.AddAsync(discount);
            await _context.SaveChangesAsync();
        }

        //public async Task UpdateDiscountAsync(Discount discount)
        //{
        //   var existingDiscount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountID == discount.DiscountID);
        //    if (existingDiscount == null)
        //    {
        //        throw new KeyNotFoundException($"Discount with ID {discount.DiscountID} not found.");
        //    }
        //    _context.Discounts.Update(discount);
        //    await _context.SaveChangesAsync();
        //}

        public async Task UpdateDiscountAsync(Discount discount)
        {
            var existingDiscount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountID == discount.DiscountID);
            if (existingDiscount == null)
            {
                throw new KeyNotFoundException($"Discount with ID {discount.DiscountID} not found.");
            }

            existingDiscount.Name = discount.Name;
            existingDiscount.Describe = discount.Describe;
            existingDiscount.DiscountRate = discount.DiscountRate;
            existingDiscount.ApplicableProduct = discount.ApplicableProduct;
            existingDiscount.Requirement = discount.Requirement;
            existingDiscount.StartDate = discount.StartDate;
            existingDiscount.EndDate = discount.EndDate;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteDiscountAsync(Guid discountId)
        {
            var discount = await _context.Discounts.FirstOrDefaultAsync(d => d.DiscountID == discountId);
            if (discount == null)
            {
                throw new KeyNotFoundException($"Discount with ID {discountId} not found.");
            }
            _context.Discounts.Remove(discount);
            await _context.SaveChangesAsync();
        }
    }
}
