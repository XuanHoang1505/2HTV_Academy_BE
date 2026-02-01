using App.DTOs;

namespace App.Services.Interfaces
{
    public interface IProgressService
    {
        Task<CourseProgressDTO> CompleteProgressAsync(int enrollmentId, int lectureId);
        Task ProgressPercentageAsync(int enrollmentId);
    }
}
