using App.Domain.Models;

namespace App.Repositories.Interfaces
{
    public interface IProgressRepository
    {
        Task<CourseProgress> CompleteProgressAsync(int enrollmentId, int lectureId);
        Task<int> CountCompletedLecturesByEnrollmentIdAsync(int enrollmentId);
        Task<int> TotalLecturesByCourseIdAsync(int enrollmentId);
    }
}
