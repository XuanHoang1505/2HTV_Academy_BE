using App.DTOs;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int? page, [FromQuery] int? limit)
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

            var users = await _userService.GetAllUsersAsync(page, limit, queryParams);
            
            return Ok(new
            {
                success = true,
                message = "Lấy danh sách users thành công ",
                data = users.Data,
                pagination = new
                {
                    total = users.Total,
                    totalPages = page.HasValue ? users.TotalPages : null,
                    currentPage = page.HasValue ? users.CurrentPage : null,
                    limit = page.HasValue ? users.Limit : null
                }
            }
            );
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Không tìm thấy user với id {id}"
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser(UserDTO dto)
        {
            var user = await _userService.CreateUserAsync(dto);

            return Ok(new
            {
                success = true,
                data = user,
                message = "Tạo user thành công"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromForm] UserDTO dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Không tìm thấy user với id {id}"
                });
            }

            return Ok(new
            {
                success = true,

                data = user,
                message = "Cập nhật user thành công"
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userService.DeleteUserAsync(id);

            if (!user)
                return NotFound(new
                {
                    success = false,
                    message = "Xóa thất bại"
                });

            return Ok(new
            {
                success = true,
                message = "Xóa user thành công ",
                data = user
            }
              );
        }

        [HttpPut("lock/{id}")]
        public async Task<IActionResult> LockUser(string id)
        {
            var user = await _userService.LockUserAsync(id);

            if (!user)
                return NotFound(new
                {
                    success = false,
                    message = "khóa thất bại"
                });

            return Ok(new
            {
                success = true,
                message = "Khóa user thành công ",
                data = user
            }
              );
        }

        [HttpPut("unlock/{id}")]
        public async Task<IActionResult> UnLockUser(string id)
        {
            var user = await _userService.UnLockUserAsync(id);

            if (!user)
                return NotFound(new
                {
                    success = false,
                    message = "Mở khóa thất bại"
                });

            return Ok(new
            {
                success = true,
                message = " Mở Khóa user thành công ",
                data = user
            }
              );
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Unauthorized"
                });
            }
            var user = await _userService.GetProfileAsync(userId);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy thông tin người dùng"
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserDTO dto)
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Unauthorized"
                });
            }

            var user = await _userService.UpdateUserAsync(userId, dto);

            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Không tìm thấy user với id {userId}"
                });
            }

            return Ok(new
            {
                success = true,
                data = user,
                message = "Cập nhật profile thành công"
            });
        }
    }
}