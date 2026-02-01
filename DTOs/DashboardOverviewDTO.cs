namespace App.DTOs
{
        public class DashboardOverviewDTO
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<MonthlyRevenueDTO> MonthlyRevenue { get; set; } = new();
        public List<MonthlyStudentDTO> MonthlyStudents { get; set; } = new();
        public List<CourseStatsDTO> CourseStats { get; set; } = new();
        public List<CourseStatsDTO> TopCourses { get; set; } = new();
    }
}