using App.Data;
using App.Domain.Enums;
using App.DTOs;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Repositories.Implementations
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDBContext _context;

        public DashboardRepository(AppDBContext context)
        {
            _context = context;
        }

        public async Task<DashboardOverviewDTO> GetDashboardOverview(int currentYear)
        {
            var result = new DashboardOverviewDTO();

            result.TotalCourses = await _context.Courses.CountAsync();

            result.TotalStudents = await _context.Enrollments
                .Select(e => e.UserId)
                .Distinct()
                .CountAsync();

            result.TotalRevenue =
                await _context.Purchases
                    .Where(p => p.Status == PurchaseStatus.Completed)
                    .SumAsync(p => (decimal?)p.Amount) ?? 0;

            var revenueData = await _context.Purchases
                .Where(p => p.Status == PurchaseStatus.Completed && p.CreatedAt.Year == currentYear)
                .GroupBy(p => p.CreatedAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.Amount)
                })
                .ToListAsync();

            result.MonthlyRevenue = Enumerable.Range(1, 12)
                .Select(month => new MonthlyRevenueDTO
                {
                    Year = currentYear,
                    Month = month,
                    Revenue = revenueData.FirstOrDefault(r => r.Month == month)?.Revenue ?? 0
                })
                .ToList();

            var studentData = await _context.Enrollments
                .Where(e => e.EnrolledAt.Year == currentYear)
                .GroupBy(e => e.EnrolledAt.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    StudentCount = g.Count()
                })
                .ToListAsync();

            result.MonthlyStudents = Enumerable.Range(1, 12)
                .Select(month => new MonthlyStudentDTO
                {
                    Year = currentYear,
                    Month = month,
                    StudentCount = studentData.FirstOrDefault(s => s.Month == month)?.StudentCount ?? 0
                })
                .ToList();

            var stats = await _context.Courses
                .Select(c => new CourseStatsDTO
                {
                    Id = c.Id,
                    CourseTitle = c.CourseTitle,
                    Students = c.Enrollments.Count,
                    Revenue = c.PurchaseItems
                                .Where(pi => pi.Purchase.Status == PurchaseStatus.Completed)
                                .Sum(pi => (decimal?)pi.Price) ?? 0
                })
                .ToListAsync();

            result.CourseStats = stats;

            result.TopCourses = stats
                .OrderByDescending(s => s.Students)
                .Take(5)
                .ToList();

            return result;
        }
    }
}