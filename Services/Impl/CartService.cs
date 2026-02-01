using App.Domain.Models;
using App.DTOs;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using AutoMapper;

namespace App.Services.Implementations
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IMapper _mapper;

        public CartService(
            ICartRepository cartRepository,
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IMapper mapper)
        {
            _cartRepository = cartRepository;
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _mapper = mapper;
        }

        public async Task<CartDTO> GetCartByUserIdAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new CartDTO
                {
                    CartItems = new List<CartItemDTO>(),
                    TotalPrice = 0,
                    ItemCount = 0
                };
            }

            var cartDto = new CartDTO
            {
                Id = cart.Id,
                TotalPrice = cart.CartItems.Sum(ci => ci.Price),
                ItemCount = cart.CartItems.Count,
                CartItems = cart.CartItems.Select(ci => new CartItemDTO
                {
                    Id = ci.Id,
                    CourseId = ci.CourseId,
                    CourseName = ci.Course.CourseTitle,
                    CourseImage = ci.Course.CourseThumbnail,
                    Price = ci.Price,
                    Discount = ci.Course.Discount,
                    AddedAt = ci.AddedAt
                }).ToList()
            };

            return cartDto;
        }

        public async Task AddCourseToCartAsync(string userId, int courseId)
        {
            var course = await _courseRepository.GetByIdAsync(courseId);
            if (course == null)
                throw new AppException(ErrorCode.CourseNotFound, "Khóa học không tồn tại");

            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = await _cartRepository.AddCartAsync(userId);
                await _cartRepository.SaveChangesAsync();
            }

            var existingItem = await _cartRepository.GetCartItemAsync(cart.Id, courseId);
            if (existingItem != null)
                throw new AppException(ErrorCode.CourseAlreadyInCart, "Khóa học đã có trong giỏ hàng");

            var isEnrolled = await _enrollmentRepository.IsUserEnrolledInCourseAsync(userId, courseId);
            if (isEnrolled)
                throw new AppException(ErrorCode.CourseAlreadyEnrolled, "Bạn đã đăng ký khóa học này");

            var cartItem = new CartItem
            {
                CartId = cart.Id,
                CourseId = courseId,
                Price = course.CoursePrice * (1 - course.Discount / 100m),
                AddedAt = DateTime.Now
            };

            await _cartRepository.AddCartItemAsync(cartItem);

            cart.TotalPrice = await _cartRepository.CalculateCartTotalAsync(cart.Id);
            await _cartRepository.UpdateCartAsync(cart);

            await _cartRepository.SaveChangesAsync();
        }

        public async Task RemoveCourseFromCartAsync(string userId, int courseId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
                throw new AppException(ErrorCode.CartNotFound, "Giỏ hàng không tồn tại");

            var cartItem = await _cartRepository.GetCartItemAsync(cart.Id, courseId);
            if (cartItem == null)
                throw new AppException(ErrorCode.CartItemNotFound, "Không tìm thấy khóa học trong giỏ hàng");

            _cartRepository.RemoveCartItem(cartItem);
            await _cartRepository.SaveChangesAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);
            if (cart == null)
                throw new AppException(ErrorCode.CartNotFound, "Giỏ hàng không tồn tại");

            await _cartRepository.ClearCartAsync(userId);
            await _cartRepository.SaveChangesAsync();
        }

        public async Task<CartSummaryDTO> GetCartSummaryAsync(string userId)
        {
            var cart = await _cartRepository.GetCartByUserIdAsync(userId);

            if (cart == null)
            {
                return new CartSummaryDTO
                {
                    ItemCount = 0,
                    Total = 0
                };
            }

            return new CartSummaryDTO
            {
                ItemCount = cart.CartItems.Count,
                Total = cart.CartItems.Sum(ci => ci.Price)
            };
        }

        public async Task<bool> IsCourseInCartAsync(string userId, int courseId)
        {
            return await _cartRepository.IsCourseInCartAsync(userId, courseId);
        }

        public async Task<decimal> GetCartTotalAsync(int cartId)
        {
            var cart = await _cartRepository.GetCartByIdAsync(cartId);
            if (cart == null)
                throw new AppException(ErrorCode.CartNotFound, "Giỏ hàng không tồn tại");

            return await _cartRepository.GetCartTotalAsync(cartId);
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            return await _cartRepository.GetCartItemCountAsync(userId);
        }
    }
}