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
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddReviewAsync(Review review)
        {
            var currentReview = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == review.ReviewID);
            if (currentReview != null)
            {
                throw new InvalidOperationException($"Review with ID {review.ReviewID} already exists.");
            }
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateReviewAsync(Review review)
        {
            var currentReview = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewID == review.ReviewID);
            if (currentReview == null)
            {
                throw new KeyNotFoundException($"Review with ID {review.ReviewID} not found.");
            }
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
        }

        public async Task<Review> GetReviewByIdAsync(Guid reviewId)
        {
            var review = await _context.Reviews.Where(r => r.ReviewID == reviewId).FirstOrDefaultAsync();
            if (review == null)
            {
                throw new KeyNotFoundException($"Review with ID:{reviewId} not found");
            }
            return review;
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            var listReview = await _context.Reviews.Where(r => r.ProductID == productId).ToListAsync();
            return listReview;
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            var listReview = await _context.Reviews.ToListAsync();
            if (listReview == null || listReview.Count == 0)
            {
                throw new KeyNotFoundException("No review found");
            }
            return listReview;
        }

        Task<IEnumerable<Review>> IReviewService.GetAllReviewsAsync()
        {
            throw new NotImplementedException();
        }
    }
}
