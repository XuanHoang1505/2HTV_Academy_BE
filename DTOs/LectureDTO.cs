namespace App.DTOs
{
    public class LectureDTO
    {
        public int? Id { get; set; }
        public string? LectureTitle { get; set; } = null!;
        public int? LectureDuration { get; set; }
        public string? LectureUrl { get; set; }
        public bool? IsPreviewFree { get; set; }
        public int LectureOrder { get; set; }
        public int ChapterId { get; set; }
        public bool IsCompleted { get; set; }
    }
}