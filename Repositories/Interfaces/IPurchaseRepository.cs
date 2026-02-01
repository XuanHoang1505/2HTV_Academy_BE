using App.Domain.Models;
using X.PagedList;

public interface IPurchaseRepository
{
    Task<IEnumerable<Purchase>> AllPurchasesAsync();
    Task<IPagedList<Purchase>> GetPagedPurchasesAsync(int page, int limit);
    Task<Purchase?> GetByIdAsync(int id);
    Task<IEnumerable<PurchaseItem>> GetPurchaseItemByPurchaseIdAsync(int purchaseId);
    Task<Purchase> CreateAsync(Purchase purchase);
    Task UpdatePurchaseAsync(Purchase purchase);
    Task DeletePurchaseAsync(int id);
    Task<IEnumerable<Purchase>> GetAllPurchaseByUserIdAsync(string userId);
}