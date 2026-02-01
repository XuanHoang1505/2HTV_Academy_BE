public class PurchaseItemDTO
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Discount { get; set; }
}
