using App.Domain.Models;
using X.PagedList;

namespace App.Repositories.Interfaces
{
    public interface ILectureRepository
    {
        Task<Lecture?> GetByIdAsync(int id);
        Task<Lecture?> GetByTitleAsync(string lectureTitle);
        Task<IEnumerable<Lecture>> AllAsync();
        Task<IPagedList<Lecture>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null);
        Task<Lecture> AddAsync(Lecture lecture);
        Task UpdateAsync(Lecture lecture);
        Task DeleteAsync(int id);
        Task<int> GetMaxOrderByChapterIdAsync(int chapterId);
    }
}