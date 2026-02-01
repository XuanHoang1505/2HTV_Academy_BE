using App.Data;
using App.Domain.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly AppDBContext _context;

    public PurchaseRepository(AppDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Purchase>> AllPurchasesAsync()
    {
        return await _context.Purchases.ToListAsync();
    }

    public async Task<Purchase?> GetByIdAsync(int id)
    {
        return await _context.Purchases
            .Include(p => p.User)
            .Include(p => p.PurchaseItems)
            .ThenInclude(pi => pi.Course)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<PurchaseItem>> GetPurchaseItemByPurchaseIdAsync(int purchaseId)
    {
        return await _context.PurchaseItems
            .Where(p => p.PurchaseId == purchaseId)
            .Include(p => p.Course)
            .ToListAsync();
    }

    public async Task<Purchase> CreateAsync(Purchase purchase)
    {
        await _context.Purchases.AddAsync(purchase);
        await _context.SaveChangesAsync();
        return purchase;
    }
    public async Task UpdatePurchaseAsync(Purchase purchase)
    {
        _context.Purchases.Update(purchase);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePurchaseAsync(int id)
    {
        var purchase = await _context.Purchases
            .Include(p => p.PurchaseItems)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (purchase != null)
        {
            _context.PurchaseItems.RemoveRange(purchase.PurchaseItems);
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Purchase>> GetAllPurchaseByUserIdAsync(string userId)
    {
        return await _context.Purchases
            .Where(p => p.UserId == userId)
            .ToListAsync();
    }

    public async Task<IPagedList<Purchase>> GetPagedPurchasesAsync(int page, int limit)
    {
        return await _context.Purchases.ToPagedListAsync(page, limit);
    }
}