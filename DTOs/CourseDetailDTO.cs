using App.Domain.Enums;
using App.DTOs;

public class CourseDetailDTO
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string CourseDescription { get; set; } = null!;
    public string? ShortDescription { get; set; }
    public Level Level { get; set; }
    public Language Language { get; set; }
    public CourseStatus Status { get; set; }
    public string? CourseThumbnail { get; set; }
    public decimal CoursePrice { get; set; }
    public bool IsPublished { get; set; }
    public int Discount { get; set; }
    public int TotalLectures { get; set; }
    public int TotalStudents { get; set; }
    public int TotalDuration { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public string EducatorName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public List<ChapterCurriculumDTO> Curriculum { get; set; } = new();
}
