namespace App.DTOs;

public class CourseFilterDTO
{
    public string? Keyword { get; set; }
    public int? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsPublished { get; set; }

    public string? SortBy { get; set; }    // "title", "price"
    public bool SortDesc { get; set; } = false;
}


