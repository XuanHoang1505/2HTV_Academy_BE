using App.DTOs;
using App.Services;
using App.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly OtpService _otpService;

        public AuthController(IUserService userService, OtpService otpService)
        {
            _userService = userService;
            _otpService = otpService;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var response = await _userService.LoginAsync(loginDTO.UserName, loginDTO.Password);

            if (response?.AccessToken == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Sai thông tin đăng nhập."
                });
            }

            return Ok(new
            {
                success = true,
                data = response,
                message = "Đăng nhập thành công"
            });
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            var result = await _userService.RegisterUserAsync(registerDTO);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Đăng ký thất bại. Vui lòng thử lại."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Đăng ký thành công, vui lòng kiểm tra email để xác minh OTP."
            });
        }

        // POST /api/auth/verify-password
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordDTO dto)
        {
            var isValid = await _userService.VerifyPasswordAsync(dto.UserId, dto.Password);

            if (!isValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Mật khẩu không đúng."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Mật khẩu hợp lệ."
            });
        }

        // POST /api/auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO forgotPasswordDTO)
        {
            var isSend = await _userService.SendResetPasswordEmailAsync(forgotPasswordDTO.Email);

            if (!isSend)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể gửi mã OTP. Vui lòng thử lại sau."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Mã OTP đã được gửi đến email của bạn."
            });
        }

        // POST /api/auth/verify-otp
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO dto)
        {
            var isValid = _otpService.ValidateOtp(dto.Email, dto.Otp);

            if (!isValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP không đúng hoặc đã hết hạn."
                });
            }

            var result = await _userService.ConfirmEmailAsync(dto.Email);
            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể xác nhận email. Vui lòng thử lại."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xác nhận email thành công. Bạn có thể đăng nhập ngay bây giờ."
            });
        }

        // POST /api/auth/verify-otp-reset-password
        [HttpPost("verify-otp-reset-password")]
        public IActionResult VerifyOtpResetPassword([FromBody] VerifyOtpDTO dto)
        {
            var isValid = _otpService.ValidateOtp(dto.Email, dto.Otp);

            if (!isValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "OTP không đúng hoặc đã hết hạn."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Xác thực OTP thành công. Bạn có thể đặt lại mật khẩu."
            });
        }

        // POST /api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            var success = await _userService.ResetPasswordAsync(dto.Email, dto.NewPassword);

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể đặt lại mật khẩu."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Đặt lại mật khẩu thành công."
            });
        }

        // POST /api/auth/change-password
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto)
        {
            var success = await _userService.ChangePasswordAsync(
                dto.UserId,
                dto.CurrentPassword,
                dto.NewPassword
            );

            if (!success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Thay đổi mật khẩu thất bại."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Thay đổi mật khẩu thành công."
            });
        }

        // POST /api/auth/resend-otp
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] SendOtpDTO dto)
        {
            try
            {
                await _otpService.SendOtp(dto.Email);

                return Ok(new
                {
                    success = true,
                    message = "OTP mới đã được gửi thành công."
                });
            }
            catch
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không thể gửi lại OTP."
                });
            }
        }

        // POST /api/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDTO logoutDTO)
        {
            await _userService.LogoutAsync(logoutDTO.UserId);

            return Ok(new
            {
                success = true,
                message = "Đăng xuất thành công."
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst("userId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Không tìm thấy thông tin người dùng."
                });
            }

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Không tìm thấy người dùng."
                });
            }

            return Ok(new
            {
                success = true,
                data = user
            });
        }
    }
}