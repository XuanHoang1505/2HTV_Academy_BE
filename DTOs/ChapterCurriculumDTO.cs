namespace App.DTOs
{
    public class ChapterCurriculumDTO
    {
        public int Id { get; set; }
        public int ChapterOrder { get; set; }
        public string ChapterTitle { get; set; } = null!;
        public string? ChapterDescription { get; set; } = null!;
        public List<LectureDTO> Lectures { get; set; } = new List<LectureDTO>();
    }
}