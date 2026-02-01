using App.DTOs;

namespace App.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartDTO> GetCartByUserIdAsync(string userId);
        Task AddCourseToCartAsync(string userId, int courseId);
        Task RemoveCourseFromCartAsync(string userId, int courseId);
        Task ClearCartAsync(string userId);
        Task<CartSummaryDTO> GetCartSummaryAsync(string userId);
        Task<bool> IsCourseInCartAsync(string userId, int courseId);
        Task<decimal> GetCartTotalAsync(int cartId);
        Task<int> GetCartItemCountAsync(string userId);
    }
}