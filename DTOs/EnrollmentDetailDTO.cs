using App.Domain.Enums;

namespace App.DTOs
{
    public class EnrollmentDetailDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string UserAvatar { get; set; } = null!;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string CourseThumbnail { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Progress { get; set; }
        public EnrollmentStatus Status { get; set; }
        public bool IsExpired { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public CourseDetailDTO Course { get; set; } = null!;
        public UserReviewDTO? UserReview { get; set; }
    }
}