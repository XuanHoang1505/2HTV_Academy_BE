using App.Domain.Enums;

namespace App.DTOs
{
    public class UpdatePurchaseStatusDTO
    {
        public PurchaseStatus Status { get; set; }
        public string TransactionId { get; set; }
    }
}