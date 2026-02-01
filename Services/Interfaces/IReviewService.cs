using App.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewDTO> CreateReviewAsync(CreateReviewDTO dto);
        Task<ReviewDTO> GetReviewByIdAsync(int id);
        Task<PagedResult<ReviewDTO>> GetAllReviewsAsync(int? page, int? limit);
        Task<IEnumerable<ReviewDTO>> GetReviewsByCourseIdAsync(int courseId);
        Task<IEnumerable<ReviewDTO>> GetReviewsByUserIdAsync(string userId);
        Task<ReviewDTO> UpdateReviewAsync(int id, UpdateReviewDTO dto, string userId);
        Task<bool> DeleteReviewAsync(int id, string userId);
        Task<CourseReviewStatsDTO> GetCourseReviewStatsAsync(int courseId);

        Task<ReviewDTO> HideReviewAsync(int id);
        Task<ReviewDTO> ShowReviewAsync(int id);
        Task<bool> UserHasReviewedCourseAsync(string userId, int courseId);

        Task TotalReviewsAsync(int courseId);
        Task AverageRatingAsync(int courseId);
    }
}