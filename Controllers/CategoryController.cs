using App.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using App.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace App.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] int? page,
            [FromQuery] int? limit)
        {
            if (limit.HasValue && (limit <= 0 || limit > 100))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Limit phải là số dương và không vượt quá 100"
                });
            }

            if (page.HasValue && page <= 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Page phải là số dương"
                });
            }

            var queryParams = HttpContext.Request.Query
                .Where(q => q.Key != "page" && q.Key != "limit")
                .ToDictionary(q => q.Key, q => q.Value.ToString());

            var result = await _categoryService.GetAllAsync(page, limit, queryParams);

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách danh mục thành công",
                pagination = limit.HasValue && page.HasValue
                    ? new
                    {
                        total = result.Total,
                        page = result.CurrentPage,
                        limit = result.Limit,
                        totalPages = result.TotalPages
                    }
                    : (object)new { total = result.Total },
                data = result.Data
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdCategory(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(new
            {
                success = true,
                message = "Lấy danh mục thành công ",
                data = category
            }
            );
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory(CategoryDTO dto)
        {
            var category = await _categoryService.CreateAsync(dto);

            if (category == null)
                return NotFound(new
                {
                    success = false,
                    message = "Tạo danh mục không thành công"
                });

            return Ok(new
            {
                success = true,
                message = "Tạo danh mục thành công ",
                data = category
            }
               );
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(int id, CategoryDTO dto)
        {
            var category = await _categoryService.UpdateAsync(id, dto);

            if (category == null)
                return NotFound(new
                {
                    success = false,
                    message = "Cập nhật không thành công"
                });

            return Ok(new
            {
                success = true,
                message = "Sửa danh mục thành công ",
                data = category
            }
               );
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryService.DeleteAsync(id);
            if (!category)
                return NotFound(new
                {
                    success = false,
                    message = "Xóa thất bại"
                });

            return Ok(new
            {
                success = true,
                message = "Xóa danh mục thành công ",
                data = category
            }
        );
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetCategoryBySlug(string slug)
        {
            var category = await _categoryService.GetBySlugAsync(slug);
            return Ok(new
            {
                success = true,
                message = "Lấy danh mục thành công ",
                data = category
            }
           );
        }
    }
}
