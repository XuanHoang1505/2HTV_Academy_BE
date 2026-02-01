using App.Data;
using App.Domain.Enums;

namespace App.Domain.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.VnPay;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}
