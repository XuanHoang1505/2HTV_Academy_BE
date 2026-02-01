using App.Domain.Enums;

public class PurchaseDTO
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public decimal? Amount { get; set; }
    public PurchaseStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Email { get; set; }

    public List<PurchaseItemDTO>? Items { get; set; }
}
