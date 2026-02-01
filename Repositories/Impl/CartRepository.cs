using App.Data;
using App.Domain.Models;
using App.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace App.Repositories.Implementations
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDBContext _context;

        public CartRepository(AppDBContext context)
        {
            _context = context;
        }

        // Lấy giỏ hàng theo UserId
        public async Task<Cart?> GetCartByUserIdAsync(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // Lấy giỏ hàng theo CartId
        public async Task<Cart?> GetCartByIdAsync(int cartId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Course)
                .FirstOrDefaultAsync(c => c.Id == cartId);
        }

        // Tạo giỏ hàng mới
        public async Task<Cart> AddCartAsync(string userId)
        {
            var cart = new Cart { UserId = userId };
            await _context.Carts.AddAsync(cart);
            return cart;
        }

        // Thêm khóa học vào giỏ
        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.CartItems.AddAsync(cartItem);
        }

        // Lấy CartItem theo CartId và CourseId
        public async Task<CartItem?> GetCartItemAsync(int cartId, int courseId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.CourseId == courseId);
        }

        // Kiểm tra khóa học có trong giỏ chưa
        public async Task<bool> IsCourseInCartAsync(string userId, int courseId)
        {
            return await _context.CartItems
                .AnyAsync(ci => ci.Cart.UserId == userId && ci.CourseId == courseId);
        }

        // Xóa khóa học khỏi giỏ
        public void RemoveCartItem(CartItem item)
        {
            _context.CartItems.Remove(item);
        }

        // Xóa toàn bộ giỏ hàng
        public async Task ClearCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
            }
            cart.TotalPrice = 0;
            _context.Carts.Update(cart);
        }

        // Tính tổng tiền giỏ hàng
        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => ci.Price);
        }

        // Đếm số khóa học trong giỏ
        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.CartItems.Count ?? 0;
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId)
        {
            return await _context.CartItems
                .Where(ci => ci.CartId == cartId)
                .SumAsync(ci => ci.Price);
        }
        // Lưu thay đổi
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}