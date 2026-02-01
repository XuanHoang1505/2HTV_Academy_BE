using App.Domain.Models;
using X.PagedList;

namespace App.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetBySlugAsync(string slug);
        Task<bool> ExistsBySlugAsync(string slug);
        Task<IPagedList<Category>> GetAllAsync(int page, int limit, Dictionary<string, string>? filters = null);
        Task<IEnumerable<Category>> AllCategoriesAsync();
        Task<Category> AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}
