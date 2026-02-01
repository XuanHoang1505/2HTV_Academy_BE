using App.DTOs;
using App.Repositories.Interfaces;
using App.Services.Interfaces;

namespace App.Services.Implementations
{
    public class ProgressService : IProgressService
    {
        private readonly IProgressRepository _progressRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public ProgressService(IProgressRepository progressRepository, IEnrollmentRepository enrollmentRepository)
        {
            _progressRepository = progressRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public async Task<CourseProgressDTO> CompleteProgressAsync(int enrollmentId, int lectureId)
        {
            var progress = await _progressRepository.CompleteProgressAsync(enrollmentId, lectureId);
            await ProgressPercentageAsync(enrollmentId);
            return new CourseProgressDTO
            {
                EnrollmentIdId = progress.EnrollmentId,
                LectureId = progress.LectureId
            };
        }

        public async Task ProgressPercentageAsync(int enrollmentId)
        {
            var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
            if (enrollment == null)
            {
                throw new Exception("Enrollment not found");
            }

            var completedLectures = await _progressRepository.CountCompletedLecturesByEnrollmentIdAsync(enrollmentId);

            var totalLectures = await _progressRepository.TotalLecturesByCourseIdAsync(enrollmentId);


            enrollment.Progress = (int)Math.Round(completedLectures * 100.0 / totalLectures);
            await _enrollmentRepository.UpdateAsync(enrollment);
        }
    }
}
