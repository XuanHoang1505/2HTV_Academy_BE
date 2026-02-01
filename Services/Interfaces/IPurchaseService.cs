using App.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace App.Services.Interfaces
{
    public interface IPurchaseService
    {
        Task<PagedResult<PurchaseDTO>> GetAllPurchasesAsync(int? page, int? limit);
        Task<IEnumerable<PurchaseItemDTO>> GetPurchaseItemByPurchaseIdAsync(int purchaseId);
        Task<PurchaseDTO> GetPurchaseByIdAsync(int purchaseId);
        Task<PurchaseDTO> CreatePurchaseAsync(CreatePurchaseDTO dto);
        Task<PurchaseDTO> UpdatePurchaseAsync(int id, PurchaseDTO dto);
        Task<bool> DeletePurchaseAsync(int id);
        Task<PurchaseDTO> UpdatePurchaseStatusAsync(int id, UpdatePurchaseStatusDTO dto);

        Task<IEnumerable<PurchaseDTO>> GetPurchasesByUserIdAsync(string userId);
    }
}