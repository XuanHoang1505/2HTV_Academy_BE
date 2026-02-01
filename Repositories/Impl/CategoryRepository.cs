using App.Data;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;
using X.PagedList.Extensions;

namespace App.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDBContext _context;
        private readonly string[] _searchableFields = { "Name", "Description", "Slug" };

        public CategoryRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IPagedList<Category>> GetAllAsync(
            int page,
            int limit,
            Dictionary<string, string>? filters = null)
        {
            var query = _context.Categories.AsQueryable();
            query = ApplyFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        private IQueryable<Category> ApplyFilters(
            IQueryable<Category> query,
            Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            if (filters.ContainsKey("search") && !string.IsNullOrEmpty(filters["search"]))
            {
                var searchTerm = filters["search"];
                query = query.Where(c =>
                    EF.Functions.Like(c.Name, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.Description, $"%{searchTerm}%") ||
                    EF.Functions.Like(c.Slug, $"%{searchTerm}%")
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
                    case "name":
                        query = query.Where(c => EF.Functions.Like(c.Name, $"%{value}%"));
                        break;
                    case "description":
                        query = query.Where(c => EF.Functions.Like(c.Description, $"%{value}%"));
                        break;
                    case "slug":
                        query = query.Where(c => EF.Functions.Like(c.Slug, $"%{value}%"));
                        break;
                }
            }

            return query;
        }

        public async Task<Category> AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories.FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<bool> ExistsBySlugAsync(string slug)
        {
            return await _context.Categories
                .AnyAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> AllCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }
    }
}
