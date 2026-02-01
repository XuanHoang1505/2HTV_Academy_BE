using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using App.DTOs;
using App.Services.Interfaces;

namespace App.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    [Authorize]
    public class EnrollmentController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            ILogger<EnrollmentController> logger)
        {
            _enrollmentService = enrollmentService;
            _logger = logger;
        }

        public string? GetUserId()
        {
            return User.FindFirst("userId")?.Value;
        }

        [HttpGet("my-enrollments")]
        public async Task<IActionResult> GetMyEnrollments([FromQuery] int? page, [FromQuery] int? limit)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User không xác định" });
                }

                var result = await _enrollmentService.GetUserEnrollmentsAsync(userId, page, limit);
                return Ok(new 
                { 
                    success = true, 
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user enrollments");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Lấy danh sách enrollments đang active của user hiện tại
        [HttpGet("my-active-enrollments")]
        public async Task<IActionResult> GetMyActiveEnrollments()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User không xác định" });
                }

                var enrollments = await _enrollmentService.GetActiveEnrollmentsByUserAsync(userId);
                return Ok(new { success = true, data = enrollments });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active enrollments");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Kiểm tra user đã enroll khóa học chưa
        [HttpGet("check-enrollment/{courseId}")]
        public async Task<IActionResult> CheckEnrollment(int courseId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User không xác định" });
                }

                var isEnrolled = await _enrollmentService.IsUserEnrolledAsync(userId, courseId);
                
                if (isEnrolled)
                {
                    var enrollment = await _enrollmentService.GetUserEnrollmentForCourseAsync(userId, courseId);
                    return Ok(new { success = true, isEnrolled = true, enrollment });
                }

                return Ok(new { success = true, isEnrolled = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Lấy chi tiết enrollment theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEnrollmentById(int id)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    return NotFound(new { success = false, message = "Enrollment không tồn tại" });
                }

                // Kiểm tra quyền truy cập
                var userId = GetUserId();
                var isAdmin = User.IsInRole("Admin");
                
                if (enrollment.UserId != userId && !isAdmin)
                {
                    return Forbid();
                }

                return Ok(new { success = true, data = enrollment });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting enrollment");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // Cập nhật tiến độ học tập
        [HttpPut("{id}/progress")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] UpdateEnrollmentProgressDTO dto)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    return NotFound(new { success = false, message = "Enrollment không tồn tại" });
                }

                // Kiểm tra quyền
                var userId = GetUserId();
                if (enrollment.UserId != userId)
                {
                    return Forbid();
                }

                var updated = await _enrollmentService.UpdateProgressAsync(id, dto);
                return Ok(new { success = true, data = updated, message = "Cập nhật tiến độ thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating progress");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateEnrollmentStatusDTO dto)
        {
            try
            {
                var updated = await _enrollmentService.UpdateStatusAsync(id, dto);
                return Ok(new { success = true, data = updated, message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("course/{courseId}/students")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetCourseEnrollments(int courseId)
        {
            try
            {
                var enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(courseId);
                var count = await _enrollmentService.GetCourseEnrollmentCountAsync(courseId);
                
                return Ok(new 
                { 
                    success = true, 
                    data = enrollments,
                    totalActive = count 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting course enrollments");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("{id}/revoke")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RevokeAccess(int id)
        {
            try
            {
                var enrollment = await _enrollmentService.RevokeAccessAsync(id);
                return Ok(new 
                { 
                    success = true, 
                    data = enrollment, 
                    message = "Thu hồi quyền truy cập thành công" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking access");
                return BadRequest(new { success = false, message = "Lỗi khi thu hồi quyền truy cập" });
            }
        }

        [HttpPost("{id}/restore")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RestoreAccess(int id)
        {
            try
            {
                var enrollment = await _enrollmentService.RestoreAccessAsync(id);
                return Ok(new 
                { 
                    success = true, 
                    data = enrollment, 
                    message = "Khôi phục quyền truy cập thành công" 
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring access");
                return BadRequest(new { success = false, message = "Lỗi khi khôi phục quyền truy cập" });
            }
        }

        [HttpGet("check-access/{courseId}")]
        public async Task<IActionResult> CheckAccess(int courseId)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "User không xác định" });
                }

                var hasAccess = await _enrollmentService.HasActiveAccessAsync(userId, courseId);
                
                if (hasAccess)
                {
                    var enrollment = await _enrollmentService.GetUserEnrollmentForCourseAsync(userId, courseId);
                    return Ok(new 
                    { 
                        success = true, 
                        hasAccess = true, 
                        enrollment 
                    });
                }

                return Ok(new { success = true, hasAccess = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking access");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        // Xóa enrollment (soft delete) - Admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            try
            {
                var result = await _enrollmentService.DeleteEnrollmentAsync(id);
                if (!result)
                {
                    return NotFound(new { success = false, message = "Enrollment không tồn tại" });
                }

                return Ok(new { success = true, message = "Xóa enrollment thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting enrollment");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }


        // Tạo enrollment thủ công (Admin only)
        [HttpPost("manual-create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateManualEnrollment([FromBody] CreateEnrollmentDTO dto)
        {
            try
            {
                var enrollment = await _enrollmentService.CreateEnrollmentAsync(dto);
                return Ok(new { success = true, data = enrollment, message = "Tạo enrollment thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual enrollment");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}