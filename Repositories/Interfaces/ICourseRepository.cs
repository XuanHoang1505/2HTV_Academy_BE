using App.Data;
using App.Domain.Models;
using App.DTOs;
using X.PagedList;

namespace App.Repositories.Interfaces
{
    public interface ICourseRepository
    {
        Task<Course?> GetByIdAsync(int id);
        Task<IPagedList<Course>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null);
        Task<Course?> GetBySlugAsync(string slug);
        Task<IPagedList<Course>> GetAllPublishAsync(int page, int limit, Dictionary<string, string>? filters = null);
        Task<IEnumerable<Course>> AllCoursesPublishAsync();
        Task<IEnumerable<Course>> AllCoursesAsync();
        Task<Course> AddAsync(Course course);
        Task UpdateAsync(Course course);
        Task DeleteAsync(int id);
        Task<Course?> CourseDetailAsync(int id);
        Task<IEnumerable<Course>> SearchAsync(CourseFilterDTO filter);
        Task<IEnumerable<Course>> GetCoursesBestSellerAsync();
        Task<IEnumerable<Course>> GetCoursesNewestAsync();
        Task<IEnumerable<Course>> GetCoursesRatingAsync();
        Task<bool> ExistsBySlugAsync(string slug);
    }
}