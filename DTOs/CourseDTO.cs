using App.Domain.Enums;

namespace App.DTOs
{
    public class CourseDTO
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string CourseDescription { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? CourseThumbnail { get; set; }
        public string? PreviewVideo { get; set; }
        public Level Level { set; get; }
        public Language Language { set; get; }
        public decimal CoursePrice { get; set; }
        public bool? IsPublished { get; set; }
        public CourseStatus Status { set; get; }
        public int Discount { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int? TotalStudents { get; set; }
        public int? TotalDuration { get; set; }
        public int? TotalLectures { get; set; }
        public int? TotalReviews { get; set; }
        public double? AverageRating { get; set; }
        public string? EducatorId { get; set; }
        public string? EducatorName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public IFormFile? CourseThumbnailFile { get; set; }

    }
}