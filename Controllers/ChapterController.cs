using App.DTOs;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/chapters")]
    public class ChapterController : ControllerBase
    {
        readonly IChapterService _chapterService;
        public ChapterController(IChapterService chapterService)
        {
            _chapterService = chapterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChapters([FromQuery] int? page, [FromQuery] int? limit)
        {
            if (limit.HasValue && (limit <= 0 || limit > 100))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Limit phải trong khoảng 1-100"
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

            var result = await _chapterService.GetAllAsync(page, limit, queryParams);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Chapter not found"
                });
            }
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách chapter thành công",
                data = result.Data,
                pagination = new
                {
                    total = result.Total,
                    totalPages = page.HasValue ? result.TotalPages : null,
                    currentPage = page.HasValue ? result.CurrentPage : null,
                    limit = page.HasValue ? result.Limit : null
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChapterByID(int id)
        {
            var chapter = await _chapterService.GetByIdAsync(id);
            if (chapter == null)
            {
                return NotFound(new
                {
                    success = false,
                    data = (object?)null,
                    message = "Chapter not found"
                });
            }
            return Ok(new
            {
                success = true,
                data = chapter,
                message = "Lấy thông tin chapter thành công"
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateChapter([FromBody] ChapterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    data = (object?)null,
                    message = "Dữ liệu không hợp lệ"
                });

            var created = await _chapterService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetChapterByID),
                new { id = created.Id },
                new
                {
                    success = true,
                    data = created,
                    message = "Tạo chapter thành công"
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateChapter(int id, [FromBody] ChapterDTO dto)
        {
            try
            {
                var updatedChapter = await _chapterService.UpdateAsync(id, dto);
                return Ok(new
                {
                    success = true,
                    data = updatedChapter,
                    message = "Cập nhật chapter thành công"
                });
            }
            catch (AppException ex) // nếu bạn dùng AppException để báo lỗi
            {
                return BadRequest(new
                {
                    success = false,
                    data = (object?)null,
                    message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChapter(int id)
        {
            try
            {
                await _chapterService.DeleteAsync(id);
                return Ok(new
                {
                    success = true,
                    data = (object?)null,
                    message = "Xóa chapter thành công"
                });
            }
            catch (AppException ex) // nếu chapter không tồn tại hoặc lỗi business
            {
                return NotFound(new
                {
                    success = false,
                    data = (object?)null,
                    message = ex.Message
                });
            }
        }

    }
}

