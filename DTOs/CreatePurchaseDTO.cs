namespace App.DTOs
{
    public class CreatePurchaseDTO
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public List<int> CourseIds { get; set; }
    }

}