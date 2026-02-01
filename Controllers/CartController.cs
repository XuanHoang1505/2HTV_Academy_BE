using App.DTOs;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/carts")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        private string? GetUserId()
        {
            return User.FindFirst("userId")?.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var cart = await _cartService.GetCartByUserIdAsync(userId);

            return Ok(new
            {
                success = true,
                message = "Lấy giỏ hàng thành công",
                data = cart
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetCartSummary()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var summary = await _cartService.GetCartSummaryAsync(userId);

            return Ok(new
            {
                success = true,
                message = "Lấy tổng giỏ hàng thành công",
                data = summary
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            await _cartService.AddCourseToCartAsync(userId, request.CourseId);

            return Ok(new
            {
                success = true,
                message = "Đã thêm khóa học vào giỏ hàng"
            });
        }

        [HttpDelete("remove/{courseId}")]
        public async Task<IActionResult> RemoveFromCart(int courseId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            await _cartService.RemoveCourseFromCartAsync(userId, courseId);

            return Ok(new
            {
                success = true,
                message = "Đã xóa khóa học khỏi giỏ hàng"
            });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            await _cartService.ClearCartAsync(userId);

            return Ok(new
            {
                success = true,
                message = "Đã xóa toàn bộ giỏ hàng"
            });
        }

        [HttpGet("check/{courseId}")]
        public async Task<IActionResult> CheckCourseInCart(int courseId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "Unauthorized" });

            var isInCart = await _cartService.IsCourseInCartAsync(userId, courseId);

            return Ok(new
            {
                success = true,
                message = "Kiểm tra khóa học trong giỏ hàng",
                data = new { isInCart }
            });
        }
    }
}
