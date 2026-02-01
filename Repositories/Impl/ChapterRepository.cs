using App.Data;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;
namespace App.Repositories.Implementations
{
    public class ChapterRepository : IChapterRepository
    {
        private readonly AppDBContext _context;
        private readonly string[] _searchableFields = { "Title", "Description", "CourseId" };

        public ChapterRepository(AppDBContext context)
        {
            _context = context;
        }
        public async Task<Chapter> AddAsync(Chapter chapter)
        {
            await _context.Chapters.AddAsync(chapter);
            await _context.SaveChangesAsync();
            return chapter;
        }

        public async Task DeleteAsync(int id)
        {
            var chapter = await _context.Chapters.FindAsync(id);
            if (chapter != null)
            {
                _context.Chapters.Remove(chapter);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IPagedList<Chapter>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null)
        {
            var query = _context.Chapters.AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        private IQueryable<Chapter> ApplyFilters(
            IQueryable<Chapter> query,
            Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            if (filters.ContainsKey("search") && !string.IsNullOrEmpty(filters["search"]))
            {
                var searchTerm = filters["search"];
                query = query.Where(c =>
                    EF.Functions.Like(c.ChapterTitle, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.ChapterDescription, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.CourseId, $"%{searchTerm}%")
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
                        query = query.Where(c => EF.Functions.Like(c.ChapterTitle, $"%{value}%"));
                        break;
                    case "description":
                        query = query.Where(c => EF.Functions.Like(c.ChapterDescription, $"%{value}%"));
                        break;
                    case "courseId":
                        query = query.Where(c => EF.Functions.Like(c.CourseId, $"%{value}%"));
                        break;
                }
            }

            return query;
        }
        public async Task<IEnumerable<Chapter>> GetAllAsync()
        {
            return await _context.Chapters.ToListAsync();
        }

        public async Task<Chapter?> GetByIdAsync(int id)
        {
            return await _context.Chapters
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Chapter?> GetByTitleAsync(string chapterTitle)
        {
            return await _context.Chapters
                .FirstOrDefaultAsync(c => c.ChapterTitle == chapterTitle);
        }

        public async Task UpdateAsync(Chapter chapter)
        {
            _context.Chapters.Update(chapter);
            await _context.SaveChangesAsync();
        }
        
        public async Task<int> GetMaxOrderByCourseIdAsync(int courseId)
        {
            return await _context.Chapters
                .Where(c => c.CourseId == courseId)
                .MaxAsync(c => (int?)c.ChapterOrder) ?? 0;
        }
    }
}
