namespace App.DTOs
{
    public class CreateEnrollmentDTO
    {
        public string UserId { get; set; } = null!;
        public int CourseId { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}