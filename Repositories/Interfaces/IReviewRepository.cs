using App.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
namespace App.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review> CreateAsync(Review review);
        Task<Review> GetByIdAsync(int id);
        Task<Review> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Review>> AllAsync();
        Task<IPagedList<Review>> GetAllAsync(int page, int limit);
        Task<IEnumerable<Review>> GetByCourseIdAsync(int courseId);
        Task<IEnumerable<Review>> GetByUserIdAsync(string userId);
        Task<Review> UpdateAsync(Review review);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(string userId, int courseId);
        Task<int> GetTotalReviewsCountAsync(int courseId);
        Task<double> GetAverageRatingAsync(int courseId);
        Task<Dictionary<int, int>> GetRatingDistributionAsync(int courseId);
        Task<Review?> GetByUserAndCourseAsync(string userId, int courseId);
    }
}