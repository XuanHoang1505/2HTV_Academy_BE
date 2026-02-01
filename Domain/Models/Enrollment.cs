using App.Data;
using App.Domain.Enums;

namespace App.Domain.Models
{
    public class Enrollment
    {
        public int Id { get; set; } 
        public string UserId { get; set; } = null!;
        public int CourseId { get; set; } 
        public DateTime EnrolledAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int Progress { get; set; } = 0;
        public EnrollmentStatus Status { get; set; }
        public bool Deleted { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ApplicationUser User { get; set; } = null!;
        public Course Course { get; set; } = null!;
         public ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
    }
}