using System.Threading.Tasks;
using App.Data;
using App.DTOs;
using App.Enums;
using App.Providers;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using App.Utils.Exceptions;
using AutoMapper;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ISendMailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtTokenProvider _jwtTokenProvider;
        private readonly OtpService _otpService;

        private readonly CloudinaryService _cloudinaryService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            ISendMailService sendMailService,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            JwtTokenProvider jwtTokenProvider,
            IMapper mapper,
            OtpService otpService,
            CloudinaryService cloudinaryService,
            ILogger<UserService> logger
            )
        {
            _userRepository = userRepository;
            _emailService = sendMailService;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenProvider = jwtTokenProvider;
            _mapper = mapper;
            _otpService = otpService;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
        {
            var userExits = await _userRepository.IsEmailExistsAsync(userDto.Email);
            if (userExits)
                throw new AppException(ErrorCode.EmailAlreadyExists, "Email đã tồn tại");

            var user = _mapper.Map<ApplicationUser>(userDto);
            user.UserName = userDto.Email;
            user.EmailConfirmed = true;
            var defaultPassword = GenerateRandomPassword(8);

            if (userDto.ImageFile != null && userDto.ImageFile.Length > 0)
            {
                user.ImageUrl = await _cloudinaryService.UploadImageAsync(userDto.ImageFile, "user_avatar");
            }

            var result = await _userRepository.CreateUserAsync(user, defaultPassword);

            if (result == null)
                throw new AppException(ErrorCode.InternalServerError, "Tạo người dùng thất bại!");
            await _userManager.AddToRoleAsync(result, Role.Admin.ToString());

            await SendEmail(userDto, defaultPassword);
            return _mapper.Map<UserDTO>(result);
        }

        public async Task<UserDTO> UpdateUserAsync(string userId, UserDTO userDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new AppException(ErrorCode.UserNotFound, "Người dùng không tồn tại!");

            bool isEmailChanged =
                !string.IsNullOrWhiteSpace(userDto.Email) &&
                !string.Equals(userDto.Email, user.Email, StringComparison.OrdinalIgnoreCase);

            string oldEmail = user.Email;

            if (!string.IsNullOrWhiteSpace(userDto.FullName))
                user.FullName = userDto.FullName;

            if (!string.IsNullOrWhiteSpace(userDto.PhoneNumber))
                user.PhoneNumber = userDto.PhoneNumber;

            if (isEmailChanged)
            {
                user.Email = userDto.Email!.Trim();
                user.UserName = userDto.Email!.Trim();
            }

            if (userDto.ImageFile != null && userDto.ImageFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(user.ImageUrl))
                {
                    var oldPublicId = CloudinaryService.ExtractPublicId(user.ImageUrl);
                    await _cloudinaryService.DeleteImageAsync(oldPublicId);
                }

                user.ImageUrl = await _cloudinaryService
                    .UploadImageAsync(userDto.ImageFile, "user_avatar");
            }

            if(userDto.Role != null)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, userDto.Role);
            }

            await _userRepository.UpdateUserAsync(user);

            if (isEmailChanged)
                await SendEmailChangeNotification(user, oldEmail);

            var result = _mapper.Map<UserDTO>(user);
            result.Role = await _userRepository.GetUserRoleAsync(user);

            return result;
        }



        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new AppException(ErrorCode.UserNotFound, "Người dùng không tồn tại!");
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var oldPublicId = CloudinaryService.ExtractPublicId(user.ImageUrl);
                await _cloudinaryService.DeleteImageAsync(oldPublicId);
            }

            return await _userRepository.DeleteUserAsync(user);
        }

        public async Task<bool> RegisterUserAsync(RegisterDTO registerDTO)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDTO.Email);
            if (existingUser != null)
                throw new AppException(ErrorCode.EmailAlreadyExists, "Email đã tồn tại trong hệ thống!");

            if (registerDTO.Password != registerDTO.ConfirmPassword)
                throw new AppException(ErrorCode.InvalidData, "Mật khẩu xác nhận không khớp!");

            var otp = await _otpService.SendOtp(registerDTO.Email);
            if (otp == null)
                throw new AppException(ErrorCode.InternalServerError, "Không thể tạo mã OTP. Vui lòng thử lại.");

            var user = _mapper.Map<ApplicationUser>(registerDTO);
            user.UserName = registerDTO.Email;
            user.EmailConfirmed = false;

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new AppException(ErrorCode.InternalServerError, $"Đăng ký thất bại: {errors}");
            }

            await _userManager.AddToRoleAsync(user, Role.Student.ToString());
            return true;
        }

        public async Task<LoginResponse> LoginAsync(string userName, string password)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
                return new LoginResponse { AccessToken = null };

            if (user.EmailConfirmed == false)
                throw new AppException(ErrorCode.AccountNotVerified, "Tài khoản chưa được xác thực. Vui lòng kiểm tra email để xác thực tài khoản.");

            if (user.IsLocked)
                throw new AppException(ErrorCode.AccountLocked, "Tài khoản này đã bị khóa.");

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _jwtTokenProvider.GenerateToken(user.UserName, roles.ToList(), user.Id);
                var refreshToken = _jwtTokenProvider.GenerateRefreshToken();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(10);

                await _userManager.UpdateAsync(user);

                return new LoginResponse
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    Role = roles.FirstOrDefault() ?? Role.Student.ToString(),
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }

            return new LoginResponse { AccessToken = null };
        }
        public async Task<bool> ResetPasswordAsync(string email, string newPassword)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            await SendEmailNotification(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> VerifyPasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }

        public async Task LogoutAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;

            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> SendResetPasswordEmailAsync(string email)
        {
            var isExist = await _userRepository.IsEmailExistsAsync(email);
            if (!isExist)
                throw new AppException(ErrorCode.UserNotFound, "Email của bạn không tồn tại trong hệ thống");

            var otp = await _otpService.SendOtp(email);
            if (otp == null)
                return false;
            return true;
        }

        public async Task<bool> SendOtpVerifyEmail(string email)
        {
            var isExist = await _userRepository.IsEmailExistsAsync(email);
            if (!isExist)
                throw new AppException(ErrorCode.UserNotFound, "Email của bạn không tồn tại trong hệ thống");

            var otp = _otpService.SendOtp(email);
            if (otp == null)
                return false;
            return true;
        }
        public async Task<bool> ConfirmEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            if (user.EmailConfirmed)
                return true;

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        private async Task SendEmailChangeNotification(ApplicationUser user, string oldEmail)
        {
            string emailSubject = "Email đăng nhập của bạn đã được thay đổi";

            string emailBody = "<html>" +
                "<head>" +
                "<style>" +
                "    body { font-family: Arial, sans-serif; line-height: 1.6; }" +
                "    .container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }" +
                "    h1 { color: #333; }" +
                "    .info { background-color: #f9f9f9; padding: 15px; border: 1px solid #e1e1e1; border-radius: 5px; }" +
                "    .footer { margin-top: 20px; font-size: 0.9em; color: #666; text-align: center; }" +
                "    .footer strong { color: #333; }" +
                "</style>" +
                "</head>" +
                "<body>" +
                "    <div class='container'>" +
                "        <div style='text-align: center; margin-bottom: 20px;'>" +
                "            <img src='https://res.cloudinary.com/da0i2y1qu/image/upload/v1731420581/logoVertical_q1nbbl.png' alt='Hight Star Logo' style='width: 150px; height: auto;' />" +
                "        </div>" +
                "        <h1>Xin chào,</h1>" +
                "        <p>Chúng tôi muốn thông báo rằng email đăng nhập tài khoản của bạn đã được thay đổi thành công.</p>" +
                "        <div class='info'>" +
                "            <p><strong>Email cũ:</strong> " + oldEmail + "</p>" +
                "            <p><strong>Email mới:</strong> " + user.Email + "</p>" +
                "        </div>" +
                "        <p>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ ngay với chúng tôi để được hỗ trợ kịp thời.</p>" +
                "        <div class='footer'>" +
                "            <p>Trân trọng,<br><strong>Đội ngũ hỗ trợ khách hàng</strong></p>" +
                "            <p><strong>Star Movie</strong><br>" +
                "            Email: starmovieteam@gmail.com | Hotline: 0888-372-325</p>" +
                "            <p>Đây là email tự động từ phần mềm Star Movie. Vui lòng không trả lời email này.</p>" +
                "        </div>" +
                "    </div>" +
                "</body>" +
                "</html>";

            await _emailService.SendEmailAsync(oldEmail, emailSubject, emailBody);
        }

        private async Task SendEmail(UserDTO userDTO, string defaultPassword)
        {

            string emailSubject = "Tài khoản của bạn đã được tạo thành công";

            string emailBody = "<html>" +
                    "<head>" +
                    "<style>" +
                    "    body { font-family: Arial, sans-serif; line-height: 1.6; }" +
                    "    .container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }"
                    +
                    "    h1 { color: #333; }" +
                    "    .info { background-color: #f9f9f9; padding: 15px; border: 1px solid #e1e1e1; border-radius: 5px; }"
                    +
                    "    .footer { margin-top: 20px; font-size: 0.9em; color: #666; text-align: center; }" +
                    "    .footer strong { color: #333; }" +
                    "</style>" +
                    "</head>" +
                    "<body>" +
                    "    <div class='container'>" +
                    "        <div style='text-align: center; margin-bottom: 20px;'>" +
                    "            <img src='https://res.cloudinary.com/da0i2y1qu/image/upload/v1731420581/logoVertical_q1nbbl.png' alt='Hight Star Logo' style='width: 150px; height: auto;' />"
                    +
                    "        </div>" +
                    "        <h1>Kính gửi " + userDTO.FullName + ",</h1>" +
                    "        <p>Chúng tôi vui mừng thông báo rằng tài khoản của bạn đã được tạo thành công. "
                    +
                    "        Dưới đây là thông tin đăng nhập của bạn:</p>" +
                    "        <div class='info'>" +
                    "            <strong>Tên đăng nhập:</strong> " + userDTO.Email + "<br>" +
                    "            <strong>Mật khẩu:</strong> " + defaultPassword +
                    "        </div>" +
                    "        <p>Vui lòng đăng nhập và thay đổi mật khẩu ngay khi có thể để đảm bảo an toàn cho tài khoản của bạn.</p>"
                    +
                    "        <p>Nếu bạn có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi.</p>" +
                    "        <div class='footer'>" +
                    "            <p>Chân thành cảm ơn bạn,<br><strong>Đội ngũ hỗ trợ khách hàng</strong></p>"
                    +
                    "            <p><strong>Star Movie</strong><br>" +
                    "            Email: starmovieteam@gmail.com | Hotline: 0888-372-325</p>" +
                    "            <p>Đây là email tự động từ phần mềm Star Movie. Vui lòng không trả lời email này.</p>"
                    +
                    "        </div>" +
                    "    </div>" +
                    "</body>" +
                    "</html>";

            await _emailService.SendEmailAsync(userDTO.Email, emailSubject, emailBody);
        }
        private async Task SendEmailNotification(ApplicationUser user)
        {

            string emailSubject = "Mật khẩu của bạn đã được thay đổi thành công";

            string emailBody = "<html>" +
                    "<head>" +
                    "<style>" +
                    "    body { font-family: Arial, sans-serif; line-height: 1.6; }" +
                    "    .container { max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; }"
                    +
                    "    h1 { color: #333; }" +
                    "    .info { background-color: #f9f9f9; padding: 15px; border: 1px solid #e1e1e1; border-radius: 5px; }"
                    +
                    "    .footer { margin-top: 20px; font-size: 0.9em; color: #666; text-align: center; }" +
                    "    .footer strong { color: #333; }" +
                    "</style>" +
                    "</head>" +
                    "<body>" +
                    "    <div class='container'>" +
                    "        <div style='text-align: center; margin-bottom: 20px;'>" +
                    "            <img src='https://res.cloudinary.com/da0i2y1qu/image/upload/v1731420581/logoVertical_q1nbbl.png' alt='Hight Star Logo' style='width: 150px; height: auto;' />"
                    +
                    "        </div>" +
                    "        <h1>Kính gửi bạn,</h1>" +
                    "        <p>Chúng tôi xin thông báo rằng mật khẩu tài khoản của bạn đã được thay đổi thành công. Nếu bạn không yêu cầu thay đổi mật khẩu này, vui lòng liên hệ ngay với chúng tôi để đảm bảo an toàn cho tài khoản của bạn.</p>"
                    +
                    "        <div class='info'>" +
                    "            <p><strong>Tên đăng nhập:</strong> " + user.UserName + "</p>" +
                    "        </div>" +
                    "        <p>Nếu bạn cần hỗ trợ thêm, đừng ngần ngại liên hệ với chúng tôi qua thông tin bên dưới.</p>" +
                    "        <div class='footer'>" +
                    "            <p>Chân thành cảm ơn bạn,<br><strong>Đội ngũ hỗ trợ khách hàng</strong></p>" +
                    "            <p><strong>Star Movie</strong><br>" +
                    "            Email: starmovieteam@gmail.com | Hotline: 0888-372-325</p>" +
                    "            <p>Đây là email tự động từ phần mềm Star Movie. Vui lòng không trả lời email này.</p>" +
                    "        </div>" +
                    "    </div>" +
                    "</body>" +
                    "</html>";

            await _emailService.SendEmailAsync(user.Email, emailSubject, emailBody);
        }

        public async Task<UserDTO> GetUserByIdAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            var role = await _userRepository.GetUserRoleAsync(user);
            UserDTO userDto = _mapper.Map<UserDTO>(user);
            userDto.Role = role;

            if (user == null)
                throw new AppException(ErrorCode.UserNotFound, "Người dùng không tồn tại!");
            return userDto;
        }

        public async Task<PagedResult<UserDTO>> GetAllUsersAsync(int? page, int? limit, Dictionary<string, string>? filters = null)
        {
            if (page.HasValue && limit.HasValue)
            {
                var pagedUsers = await _userRepository.GetAllUsersPagedAsync(page.Value, limit.Value, filters);

                IEnumerable<UserDTO> userDTOs = _mapper.Map<IEnumerable<UserDTO>>(pagedUsers);
                foreach (var userDto in userDTOs)
                {
                    var user = await _userRepository.GetUserByIdAsync(userDto.Id!);
                    var role = await _userRepository.GetUserRoleAsync(user);
                    userDto.Role = role;
                }

                return new PagedResult<UserDTO>
                {
                    Data = userDTOs,
                    Total = pagedUsers.TotalItemCount,
                    TotalPages = pagedUsers.PageCount,
                    CurrentPage = pagedUsers.PageNumber,
                    Limit = pagedUsers.PageSize
                };
            }
            else
            {
                var users = await _userRepository.AllUsersAsync();
                return new PagedResult<UserDTO>
                {
                    Data = _mapper.Map<IEnumerable<UserDTO>>(users),
                    Total = users.Count()
                };
            }
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            return await _userRepository.LockUserAsync(userId);
        }

        public async Task<bool> UnLockUserAsync(string userId)
        {
            return await _userRepository.UnLockUserAsync(userId);
        }

        public async Task<UserDTO> GetProfileAsync(string userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            var role = await _userRepository.GetUserRoleAsync(user);
            UserDTO userDto = _mapper.Map<UserDTO>(user);
            userDto.Role = role;
            return userDto;
        }
    }
}