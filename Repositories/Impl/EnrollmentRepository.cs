using App.Data;
using App.Domain.Enums;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

namespace App.Repositories.Implementations
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly AppDBContext _context;

        public EnrollmentRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Enrollment?> GetByIdAsync(int id)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Educator)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.CourseContent)
                        .ThenInclude(cc => cc.ChapterContent)
                .Include(e => e.CourseProgresses)
                .FirstOrDefaultAsync(e => e.Id == id && !e.Deleted);
        }

        public async Task<Enrollment?> GetByUserAndCourseAsync(string userId, int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId && !e.Deleted);
        }

        public async Task<IPagedList<Enrollment>> GetByUserIdAsync(string userId, int page, int limit)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId && !e.Deleted && e.Status == EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrolledAt)
                .ToPagedListAsync(page, limit);
        }

        public async Task<IEnumerable<Enrollment>> GetByCourseIdAsync(int courseId)
        {
            return await _context.Enrollments
                .Include(e => e.User)
                .Where(e => e.CourseId == courseId && !e.Deleted)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }

        public async Task<Enrollment> CreateAsync(Enrollment enrollment)
        {
            enrollment.CreatedAt = DateTime.UtcNow;
            enrollment.UpdatedAt = DateTime.UtcNow;

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return enrollment;
        }

        public async Task<Enrollment> UpdateAsync(Enrollment enrollment)
        {
            enrollment.UpdatedAt = DateTime.UtcNow;

            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();

            return enrollment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null) return false;

            enrollment.Deleted = true;
            enrollment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(string userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && !e.Deleted);
        }

        public async Task<int> GetTotalEnrollmentsByCourseAsync(int courseId)
        {
            return await _context.Enrollments
                .CountAsync(e => e.CourseId == courseId && !e.Deleted && e.Status == EnrollmentStatus.Active);
        }

        public async Task<IEnumerable<Enrollment>> GetActiveEnrollmentsByUserAsync(string userId)
        {
            return await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.UserId == userId &&
                           !e.Deleted &&
                           e.Status == EnrollmentStatus.Active &&
                           (e.ExpiresAt == null || e.ExpiresAt > DateTime.UtcNow))
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();
        }
        public async Task<bool> IsUserEnrolledInCourseAsync(string userId, int courseId)
        {
            return await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId && !e.Deleted && e.Status == EnrollmentStatus.Active &&
                              e.ExpiresAt > DateTime.UtcNow);
        }
    }
}