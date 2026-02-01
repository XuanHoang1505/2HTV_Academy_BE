using App.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/progresses")]
    public class ProgressController : ControllerBase
    {
        readonly IProgressService _progressService;
        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        [HttpPost]
        public async Task<IActionResult> CompletedProgress(int enrollmentId, int lectureId)
        {
            var result = await _progressService.CompleteProgressAsync(enrollmentId, lectureId);
            return Ok(new
            {
                success = true,
                message = "Cập nhật tiến độ học tập thành công",
                data = result
            });
        }
    }

}
