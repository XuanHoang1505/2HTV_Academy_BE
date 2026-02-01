using App.Data;
using App.Domain.Enums;
using App.Domain.Models;
using App.DTOs;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

namespace App.Repositories.Implementations
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDBContext _context;
        private readonly string[] _searchableFields = { "Title", "Description", "Slug", "ShortDescription" };
        private readonly string[] _exactMatchFields = { "Level", "Language", "Status", "CategoryId", "InstructorId" };

        public CourseRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Educator)
                .Include(c => c.Reviews)
                .Include(c => c.CourseContent.OrderBy(ch => ch.ChapterOrder))
                    .ThenInclude(ch => ch.ChapterContent.OrderBy(l => l.LectureOrder))
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Course?> GetBySlugAsync(string slug)
        {
            return await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Educator)
                .Include(c => c.CourseContent.OrderBy(ch => ch.ChapterOrder))
                    .ThenInclude(ch => ch.ChapterContent.OrderBy(l => l.LectureOrder))
                .Where(c => c.IsPublished == true && c.Status == CourseStatus.published)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IPagedList<Course>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null)
        {
            var query = _context.Courses.Include(c => c.Category).Include(u => u.Educator).AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        private IQueryable<Course> ApplyFilters(
            IQueryable<Course> query,
            Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            // 1. Handle SEARCH (OR logic)
            if (filters.TryGetValue("search", out var searchTerm) && !string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(c =>
                    EF.Functions.Like(c.CourseTitle, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.CourseDescription, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.Slug, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.ShortDescription, $"%{searchTerm}%")
                );
            }

            // 2. Handle FILTERS (AND logic)
            foreach (var (key, value) in filters)
            {
                if (key.Equals("search", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(value))
                    continue;

                var lowerKey = key.ToLower();

                if (lowerKey == "rating" && double.TryParse(value, out var rating))
                {
                    query = query.Where(c => c.AverageRating >= rating);
                    continue;
                }

                if (lowerKey == "duration")
                {
                    var ranges = value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    query = query.Where(c =>
                        ranges.Any(r =>
                            (r == "0-5" && c.TotalDuration <= 5 * 3600) ||
                            (r == "5-10" && c.TotalDuration > 5 * 3600 && c.TotalDuration <= 10 * 3600) ||
                            (r == "10-20" && c.TotalDuration > 10 * 3600 && c.TotalDuration <= 20 * 3600) ||
                            (r == "over-20" && c.TotalDuration > 20 * 3600)
                        )
                    );

                    continue;
                }

                if (_searchableFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    query = ApplyPartialMatch(query, lowerKey, value);
                }
                else if (_exactMatchFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                {
                    query = ApplyExactMatch(query, lowerKey, value);
                }
            }

            return query;
        }

        private IQueryable<Course> ApplyPartialMatch(IQueryable<Course> query, string key, string value)
        {
            return key switch
            {
                "title" => query.Where(c => EF.Functions.Like(c.CourseTitle, $"%{value}%")),
                "description" => query.Where(c => EF.Functions.Like(c.CourseDescription, $"%{value}%")),
                "slug" => query.Where(c => EF.Functions.Like(c.Slug, $"%{value}%")),
                "shortdescription" => query.Where(c => EF.Functions.Like(c.ShortDescription, $"%{value}%")),
                _ => query
            };
        }

        private IQueryable<Course> ApplyExactMatch(IQueryable<Course> query, string key, string value)
        {
            return key switch
            {
                // Parse enum với IgnoreCase
                "level" when Enum.TryParse<Level>(value, true, out var level)
                    => query.Where(c => c.Level == level),

                "language" when Enum.TryParse<Language>(value, true, out var language)
                    => query.Where(c => c.Language == language),

                "status" when Enum.TryParse<CourseStatus>(value, true, out var status)
                    => query.Where(c => c.Status == status),

                // Parse int cho foreign keys
                "categoryid" when int.TryParse(value, out var categoryId)
                    => query.Where(c => c.CategoryId == categoryId),

                "instructorid" => query.Where(c => c.EducatorId == value),

                _ => query
            };
        }

        public async Task<Course> AddAsync(Course course)
        {
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task UpdateAsync(Course course)
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Course?> CourseDetailAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Educator)
                .Include(c => c.Category)
                .Include(c => c.CourseContent)
                .Include(c => c.PurchaseItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course;
        }


        public async Task<IEnumerable<Course>> SearchAsync(CourseFilterDTO filter)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Educator)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim().ToLower();
                query = query.Where(c =>
                    c.CourseTitle.ToLower().Contains(keyword) ||
                    c.CourseDescription.ToLower().Contains(keyword));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == filter.CategoryId.Value);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(c => c.CoursePrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(c => c.CoursePrice <= filter.MaxPrice.Value);
            }

            if (filter.IsPublished.HasValue)
            {
                query = query.Where(c => c.IsPublished == filter.IsPublished.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "price":
                        query = filter.SortDesc
                            ? query.OrderByDescending(c => c.CoursePrice)
                            : query.OrderBy(c => c.CoursePrice);
                        break;
                    case "title":
                    default:
                        query = filter.SortDesc
                            ? query.OrderByDescending(c => c.CourseTitle)
                            : query.OrderBy(c => c.CourseTitle);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(c => c.CourseTitle);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesBestSellerAsync()
        {
            return await _context.Courses
                     .Include(c => c.Category)
                     .Where(c => c.Status.Equals("published") && c.IsPublished == true)
                     .OrderByDescending(c => c.TotalStudents)
                     .Take(4)
                     .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesNewestAsync()
        {
            return await _context.Courses
                       .Include(c => c.Category)
                       .Where(c => c.Status.Equals("published") && c.IsPublished == true)
                       .OrderByDescending(c => c.CreatedAt)
                       .Take(4)
                       .ToListAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesRatingAsync()
        {
            return await _context.Courses
                       .Include(c => c.Category)
                       .Where(c => c.Status.Equals("published") && c.IsPublished == true)
                       .OrderByDescending(c => c.AverageRating)
                       .Take(4)
                       .ToListAsync();
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            return await _context.Courses
                .AnyAsync(c => c.Slug == slug);
        }

        public async Task<IPagedList<Course>> GetAllPublishAsync(int page, int limit, Dictionary<string, string>? filters = null)
        {
            var query = _context.Courses.Include(c => c.Category).Include(u => u.Educator)
                 .Where(c => c.Status == CourseStatus.published && c.IsPublished == true).AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        public async Task<IEnumerable<Course>> AllCoursesPublishAsync()
        {
            return await _context.Courses.Include(c => c.Category).Include(u => u.Educator)
                 .Where(c => c.Status == CourseStatus.published && c.IsPublished == true)
                 .ToListAsync();
        }

        public async Task<IEnumerable<Course>> AllCoursesAsync()
        {
            return await _context.Courses.Include(c => c.Category).Include(u => u.Educator)
                .ToListAsync();
        }
    }
}
