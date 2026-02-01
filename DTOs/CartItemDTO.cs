namespace App.DTOs
{
    public class CartItemDTO
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public string? CourseImage { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public DateTime AddedAt { get; set; }
    }
}