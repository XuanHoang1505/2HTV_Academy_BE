using App.Data;
using App.Domain.Models;
using App.Enums;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

namespace App.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDBContext _context;

        public ReviewRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Review> CreateAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<Review> GetByIdAsync(int id)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Review> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Course)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Review>> AllAsync()
        {
            return await _context.Reviews
                .Where(r => r.Status == ReviewStatus.Enable)
                .Include(r => r.User)
                .Include(r => r.Course)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Reviews
                .Where(r => r.Status == ReviewStatus.Enable)
                .Include(r => r.User)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Review>> GetByUserIdAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.Course)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review> UpdateAsync(Review review)
        {
            review.UpdatedAt = DateTime.Now;
            _context.Reviews.Update(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var review = await GetByIdAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string userId, int courseId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.CourseId == courseId);
        }

        public async Task<int> GetTotalReviewsCountAsync(int courseId)
        {
            return await _context.Reviews
                .Where(r => r.CourseId == courseId && r.Status == ReviewStatus.Enable)
                .CountAsync();
        }

        public async Task<double> GetAverageRatingAsync(int courseId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.CourseId == courseId && r.Status == ReviewStatus.Enable)
                .ToListAsync();

            if (!reviews.Any()) return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int courseId)
        {
            var distribution = await _context.Reviews
                .Where(r => r.CourseId == courseId)
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Rating, x => x.Count);

            // Đảm bảo có đủ 5 rating level
            for (int i = 1; i <= 5; i++)
            {
                if (!distribution.ContainsKey(i))
                {
                    distribution[i] = 0;
                }
            }

            return distribution;
        }

        public async Task<IPagedList<Review>> GetAllAsync(int page, int limit)
        {
            return await _context.Reviews
                .Where(r => r.Status == ReviewStatus.Enable)
               .Include(r => r.User)
               .Include(r => r.Course)
               .OrderByDescending(r => r.CreatedAt)
               .ToPagedListAsync(page, limit);
        }

        public async Task<Review?> GetByUserAndCourseAsync(string userId, int courseId)
        {
            return await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);
        }
    }
}