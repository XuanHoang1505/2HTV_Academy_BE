using App.Data;

namespace App.Domain.Models
{
 public class CourseProgress
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public int LectureId { get; set; }
        public Enrollment Enrollment { get; set; } = null!;
        public Lecture Lecture { get; set; } = null!;   
    }
}