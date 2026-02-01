using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App.Services.Interfaces;

namespace App.Services
{
    public class OtpService
    {
        private static readonly ConcurrentDictionary<string, OtpData> OtpStorage = new ConcurrentDictionary<string, OtpData>();
        private const int OtpExpiryMinutes = 2;
        private readonly ISendMailService _sendMailService;

        public OtpService(ISendMailService sendMailService)
        {
            _sendMailService = sendMailService;
        }

        // Tạo OTP ngẫu nhiên
        public async Task<string> SendOtp(string email)
        {
            string otp = GenerateOtp();
            var otpData = new OtpData(otp, DateTime.Now.AddMinutes(OtpExpiryMinutes));
            OtpStorage[email] = otpData;
            await SendOtpEmail(email, otp); // Gửi OTP qua email
            return otp;
        }

        // Kiểm tra OTP hợp lệ
        public bool ValidateOtp(string email, string otp)
        {
            if (OtpStorage.TryGetValue(email, out var otpData))
            {
                if (otpData.ExpiryTime > DateTime.Now && otpData.Otp == otp)
                {
                    OtpStorage.TryRemove(email, out _); // Xóa OTP sau khi sử dụng
                    return true;
                }
            }

            return false;
        }

        // Tạo OTP ngẫu nhiên 6 chữ số
        private string GenerateOtp()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        // Phương thức gửi OTP qua email
        private async Task SendOtpEmail(string to, string otpCode)
        {
            string subject = "Mã OTP của bạn từ 2HTV Academy";

            string emailBody = $@"
            <html>
            <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #ffffff; }}
                h1 {{ color: #333; font-size: 1.5em; }}
                .otp-code {{ font-size: 1.8em; font-weight: bold; color: #4CAF50; padding: 10px; background-color: #f4f4f4; border: 1px solid #ddd; border-radius: 5px; text-align: center; }}
                .footer {{ margin-top: 20px; font-size: 0.9em; color: #666; text-align: center; }}
                .footer strong {{ color: #333; }}
            </style>
            </head>
            <body>
                <div class='container'>
                    <h1>Xin chào,</h1>
                    <p>Bạn đã yêu cầu mã OTP để xác thực tài khoản tại 2HTV_Academy.</p>
                    <p>Dưới đây là mã OTP của bạn:</p>
                    <div class='otp-code'>{otpCode}</div>
                    <p>Mã OTP này có hiệu lực trong vòng <strong>2 phút</strong>. Vui lòng không chia sẻ mã này với bất kỳ ai.</p>
                    <p>Nếu bạn không yêu cầu mã OTP, vui lòng bỏ qua email này hoặc liên hệ với chúng tôi để được hỗ trợ.</p>
                    <div class='footer'>
                        <p>Trân trọng,<br><strong>Đội ngũ Hỗ trợ 2HTV Academy</strong></p>
                        <p><strong>2HTV Academy</strong><br>
                        Email: starmovieteam@gmail.com | Hotline: 0888-372-325</p>
                        <p>Đây là email tự động từ hệ thống 2HTV Academy. Đây là mail tự động vui lòng không trả lời email này.</p>
                    </div>
                </div>
            </body>
            </html>";
            
            await _sendMailService.SendEmailAsync(to, subject, emailBody);
        }

        // Lớp lưu trữ OTP và thời gian hết hạn
        private class OtpData
        {
            public string Otp { get; }
            public DateTime ExpiryTime { get; }

            public OtpData(string otp, DateTime expiryTime)
            {
                Otp = otp;
                ExpiryTime = expiryTime;
            }
        }
    }
}
