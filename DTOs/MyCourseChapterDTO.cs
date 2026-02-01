using System.Collections.Generic;
using System.Linq;

namespace App.DTOs;

public class MyCourseChapterDTO
{
    public int Id { get; set; }
    public string ChapterId { get; set; } = null!;
    public string ChapterTitle { get; set; } = null!;
    public int ChapterOrder { get; set; }
    public IEnumerable<LectureDTO> Lectures { get; set; } = Enumerable.Empty<LectureDTO>();
}

