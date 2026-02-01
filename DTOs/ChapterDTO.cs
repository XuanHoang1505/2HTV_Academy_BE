
namespace App.DTOs
{
    public class ChapterDTO
    {
        public int Id { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; } = null!;
        public string ChapterDescription { get; set; } = null!;
        public int CourseId { get; set; }
    }

}