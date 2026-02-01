namespace App.DTOs;

public class StudentCourseProgressDTO
{
    public string UserId { get; set; } = null!;
    public string? FullName { get; set; }
    public string? Email { get; set; }

    public int CourseId { get; set; }
    public bool Completed { get; set; }

    public int TotalLectures { get; set; }
    public int CompletedLectures { get; set; }
    public double ProgressPercent { get; set; }

    // Lưu raw để nếu sau này FE cần chi tiết bài học đã hoàn thành
    public string LectureCompletedRaw { get; set; } = null!;
}


