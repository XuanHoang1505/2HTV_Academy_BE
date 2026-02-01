using App.Domain.Enums;

namespace App.DTOs
{
    public class EnrollmentResponseDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int CourseId { get; set; }
        public string Slug { get; set; } = null!;
        public decimal FinalPrice { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseThumbnail { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Progress { get; set; }
        public EnrollmentStatus Status { get; set; }
        public bool IsExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}