using App.Data;

namespace App.Domain.Models
{
    public class Chapter
    {
        public int Id { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; } = null!;
        public string? ChapterDescription { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public ICollection<Lecture> ChapterContent { get; set; } = new List<Lecture>();
    }
}