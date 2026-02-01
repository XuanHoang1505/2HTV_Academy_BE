using App.DTOs;

namespace App.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDTO?> GetByIdAsync(int id);
        Task<CategoryDTO?> GetBySlugAsync(string slug);
        Task<PagedResult<CategoryDTO>> GetAllAsync(int? page, int? limit, Dictionary<string, string>? filters = null);
        Task<CategoryDTO> CreateAsync(CategoryDTO dto);
        Task<CategoryDTO> UpdateAsync(int id, CategoryDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
