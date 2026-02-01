
using App.Data;

namespace App.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
