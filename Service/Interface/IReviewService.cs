using Microsoft.EntityFrameworkCore;
using PBL3_HK4.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBL3_HK4.Service.Interface
{
    public interface IReviewService
    {
        public Task AddReviewAsync(Review review);
        public Task UpdateReviewAsync(Review review);
        public Task<Review> GetReviewByIdAsync(Guid reviewId);
        public Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId);
        public Task<IEnumerable<Review>> GetAllReviewsAsync();
    }
}
