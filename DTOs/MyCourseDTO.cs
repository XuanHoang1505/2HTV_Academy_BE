using System.Collections.Generic;
using System.Linq;

namespace App.DTOs;

public class MyCourseDTO
{
    public int Id { get; set; }
    public string CourseTitle { get; set; } = null!;
    public string CourseDescription { get; set; } = null!;
    public string? CourseThumbnail { get; set; }
    public decimal CoursePrice { get; set; }
    public int Discount { get; set; }
    public bool IsPublished { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;

    public string EducatorId { get; set; } = null!;
    public string EducatorName { get; set; } = null!;

    public int TotalChapters { get; set; }
    public int TotalLectures { get; set; }

    public IEnumerable<MyCourseChapterDTO> Chapters { get; set; } = Enumerable.Empty<MyCourseChapterDTO>();
}