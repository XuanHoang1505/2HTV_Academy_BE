namespace App.Domain.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
        public int CourseId { get; set; }
        public Course Course { get; set; }
        public decimal Price { get; set; }
        
    }
}
