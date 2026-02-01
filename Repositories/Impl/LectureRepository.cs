using App.Data;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;
using X.PagedList.Extensions;
namespace App.Repositories.Implementations
{
    public class LectureRepository : ILectureRepository
    {
        private readonly AppDBContext _context;
        private readonly string[] _searchableFields = { "Title", "Description",  "ChapterId", "CourseId" };
        public LectureRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Lecture> AddAsync(Lecture lecture)
        {
            await _context.Lectures.AddAsync(lecture);
            await _context.SaveChangesAsync();
            return lecture;
        }

        public async Task DeleteAsync(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);
            if (lecture != null)
            {
                _context.Lectures.Remove(lecture);
                await _context.SaveChangesAsync();
            }

        }

        public async Task<IEnumerable<Lecture>> AllAsync()
        {
            return await _context.Lectures
                .ToListAsync();
        }

        public async Task<Lecture?> GetByIdAsync(int id)
        {
            return await _context.Lectures
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Lecture?> GetByTitleAsync(string lectureTitle)
        {
            return await _context.Lectures
               .FirstOrDefaultAsync(l => l.LectureTitle == lectureTitle);
        }

        public async Task UpdateAsync(Lecture lecture)
        {
            _context.Lectures.Update(lecture);
            await _context.SaveChangesAsync();
        }

        public async Task<IPagedList<Lecture>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null)
        {
            var query = _context.Lectures.AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        private IQueryable<Lecture> ApplyFilters(
            IQueryable<Lecture> query,
            Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            if (filters.ContainsKey("search") && !string.IsNullOrEmpty(filters["search"]))
            {
                var searchTerm = filters["search"];
                query = query.Where(c =>
                    EF.Functions.Like(c.LectureTitle, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.LectureDescription, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.ChapterId.ToString(), $"%{searchTerm}%") ||
                    EF.Functions.Like(c.Chapter.CourseId.ToString(), $"%{searchTerm}%")
                );
            }

            foreach (var filter in filters)
            {
                var key = filter.Key;
                var value = filter.Value;

                if (key.ToLower() == "search")
                    continue;

                if (string.IsNullOrEmpty(value))
                    continue;

                // Chỉ apply filter cho searchable fields
                if (!_searchableFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                    continue;

                // Partial search (LIKE %value%)
                switch (key.ToLower())
                {
                    case "title":
                        query = query.Where(c => EF.Functions.Like(c.LectureTitle, $"%{value}%"));
                        break;
                    case "description":
                        query = query.Where(c => EF.Functions.Like(c.LectureDescription, $"%{value}%"));
                        break;
                    case "chapterId":
                        query = query.Where(c => EF.Functions.Like(c.ChapterId.ToString(), $"%{value}%"));
                        break;
                    case "courseId":
                        query = query.Where(c => EF.Functions.Like(c.Chapter.CourseId.ToString(), $"%{value}%"));
                        break;    
                }
            }

            return query;
        }
        public async Task<int> GetMaxOrderByChapterIdAsync(int chapterId)
        {
            return await _context.Lectures
                .Where(l => l.ChapterId == chapterId)
                .MaxAsync(l => (int?)l.LectureOrder) ?? 0;
        }
    }

}