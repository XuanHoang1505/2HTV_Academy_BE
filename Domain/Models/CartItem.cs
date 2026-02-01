namespace App.Domain.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart Cart { get; set; } = null!;
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}