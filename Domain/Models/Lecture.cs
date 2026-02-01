using App.Data;

namespace App.Domain.Models
{
    public class Lecture
    {
        public int Id { get; set; }
        public string LectureTitle { get; set; } = null!;
        public string? LectureDescription { get; set; }      
        public int LectureDuration { get; set; }
        public string? LectureUrl { get; set; }
        public bool IsPreviewFree { get; set; }
        public int LectureOrder { get; set; }
        public int ChapterId { get; set; }
        public Chapter Chapter { get; set; } = null!;
        public ICollection<CourseProgress> CourseProgresses { get; set; } = new List<CourseProgress>();
    }
}