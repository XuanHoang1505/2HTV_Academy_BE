using App.Data;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Repositories.Implementations
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly AppDBContext _context;

        public ProgressRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<CourseProgress> CompleteProgressAsync(int enrollmentId, int lectureId)
        {
            var progress = new CourseProgress
            {
                EnrollmentId = enrollmentId,
                LectureId = lectureId,
            };

            await _context.CourseProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();

            return progress;
        }

        public async Task<int> CountCompletedLecturesByEnrollmentIdAsync(int enrollmentId)
        {
            return await _context.CourseProgresses
                .CountAsync(cp => cp.EnrollmentId == enrollmentId);
        }

        public async Task<int> TotalLecturesByCourseIdAsync(int enrollmentId)
        {
            var enrollment = await _context.Enrollments
                    .Include(e => e.Course).ThenInclude(c => c.CourseContent).ThenInclude(ch => ch.ChapterContent)
                    .FirstOrDefaultAsync(e => e.Id == enrollmentId);

            if (enrollment == null)
                return 0;

            return enrollment.Course.CourseContent
                .Sum(ch => ch.ChapterContent.Count);
        }
    }


}