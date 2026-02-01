using System.ComponentModel.DataAnnotations;
using App.Data;
using App.Domain.Enums;

namespace App.Domain.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseTitle { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string CourseDescription { get; set; } = null!;
        public string? ShortDescription { get; set; }
        public string? CourseThumbnail { get; set; }
        public string? PreviewVideo { get; set; }
        public string EducatorId { get; set; } = null!;
        public int CategoryId { get; set; }
        public Level Level { set; get; } = Level.beginner;
        public Language Language { set; get; } = Language.vi;
        public decimal CoursePrice { get; set; }
        public int Discount { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải >= 0")]
        public int TotalLectures { get; set; } = 0;
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải >= 0")]
        public int TotalStudents { set; get; } = 0;
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải >= 0")]
        public int TotalDuration { set; get; } = 0;
        [Range(0, int.MaxValue, ErrorMessage = "Giá trị phải >= 0")]
        public int TotalReviews { set; get; } = 0;
        public double AverageRating { set; get; } = 0;
        public CourseStatus Status { set; get; } = CourseStatus.draft;
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Quan hệ
        public ApplicationUser Educator { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Chapter> CourseContent { get; set; } = new List<Chapter>();
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}