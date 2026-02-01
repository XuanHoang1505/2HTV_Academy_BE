using Microsoft.AspNetCore.Mvc;

using App.Helpers;
using App.DTOs;
using App.Domain.Enums;
using App.Services.Interfaces;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPurchaseService _purchaseService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICartService _cartService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IConfiguration config,
            IPurchaseService purchaseService,
            IEnrollmentService enrollmentService,
            ICartService cartService,
            ILogger<PaymentController> logger)
        {
            _config = config;
            _purchaseService = purchaseService;
            _enrollmentService = enrollmentService;
            _cartService = cartService;
            _logger = logger;
        }

        [HttpPost("create-payment")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePurchaseDTO request)
        {
            try
            {
                foreach (var courseId in request.CourseIds)
                {
                    var isEnrolled = await _enrollmentService.IsUserEnrolledAsync(request.UserId, courseId);

                    if (isEnrolled)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "Bạn đã đăng ký khóa học này trước đó.",
                        });
                    }
                }

                // 1. Tạo Purchase trong database trước (Status = Pending)
                var purchase = await _purchaseService.CreatePurchaseAsync(request);

                // 2. Tạo payment URL với VNPay
                var vnpay = new VnPayLibrary();

                string vnp_TmnCode = _config["VnPay:TmnCode"];
                string vnp_HashSecret = _config["VnPay:HashSecret"];
                string vnp_Url = _config["VnPay:BaseUrl"];
                string vnp_ReturnUrl = _config["VnPay:ReturnUrl"];

                // Sử dụng PurchaseId làm TxnRef để tracking
                string vnp_TxnRef = purchase.Id.ToString();

                // Thêm các tham số
                vnpay.AddRequestData("vnp_Version", "2.1.0");
                vnpay.AddRequestData("vnp_Command", "pay");
                vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
                vnpay.AddRequestData("vnp_Amount", (Convert.ToInt64(purchase.Amount * 100)).ToString());
                vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
                vnpay.AddRequestData("vnp_CurrCode", "VND");
                vnpay.AddRequestData("vnp_IpAddr", GetIpAddress());
                vnpay.AddRequestData("vnp_Locale", "vn");
                vnpay.AddRequestData("vnp_OrderInfo", $"Thanh toan don hang #{purchase.Id}");
                vnpay.AddRequestData("vnp_OrderType", "other");
                vnpay.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
                vnpay.AddRequestData("vnp_TxnRef", vnp_TxnRef);

                // Tạo URL thanh toán
                string paymentUrl = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);

                return Ok(new
                {
                    success = true,
                    paymentUrl = paymentUrl,
                    purchaseId = purchase.Id,
                    amount = purchase.Amount,
                    txnRef = vnp_TxnRef
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            try
            {
                var vnpay = new VnPayLibrary();

                // Lấy toàn bộ query parameters
                foreach (string key in Request.Query.Keys)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, Request.Query[key]);
                    }
                }

                string vnp_HashSecret = _config["VnPay:HashSecret"];
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
                string vnp_Amount = vnpay.GetResponseData("vnp_Amount");
                string vnp_TransactionNo = Request.Query["vnp_TransactionNo"];

                // Kiểm tra chữ ký
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (!checkSignature)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Chữ ký không hợp lệ"
                    });
                }

                // Parse PurchaseId từ TxnRef
                if (!int.TryParse(vnp_TxnRef, out int purchaseId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã giao dịch không hợp lệ"
                    });
                }

                // Lấy thông tin Purchase
                var purchase = await _purchaseService.GetPurchaseByIdAsync(purchaseId);
                if (purchase == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy đơn hàng"
                    });
                }

                // Kiểm tra nếu đơn hàng đã được xử lý
                if (purchase.Status == PurchaseStatus.Completed)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "Đơn hàng đã được thanh toán trước đó",
                        purchaseId = purchaseId,
                        amount = purchase.Amount
                    });
                }

                // Xử lý kết quả thanh toán
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    // Thanh toán thành công - Cập nhật status
                    var updateDto = new UpdatePurchaseStatusDTO
                    {
                        Status = PurchaseStatus.Completed,
                        TransactionId = vnp_TransactionNo
                    };

                    await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, updateDto);

                    await _cartService.ClearCartAsync(purchase.UserId);

                    // TẠO ENROLLMENT TỰ ĐỘNG CHO TẤT CẢ KHÓA HỌC
                    try
                    {
                        var enrollments = await _enrollmentService.CreateEnrollmentsFromPurchaseAsync(purchaseId);
                        _logger.LogInformation($"Created {enrollments.Count()} enrollments for Purchase {purchaseId}");
                    }
                    catch (Exception enrollEx)
                    {
                        _logger.LogError(enrollEx, $"Failed to create enrollments for Purchase {purchaseId}");
                        // Không throw exception để không ảnh hưởng đến flow thanh toán
                    }

                    _logger.LogInformation($"Payment successful for Purchase {purchaseId}");

                    return Ok(new
                    {
                        success = true,
                        message = "Thanh toán thành công",
                        purchaseId = purchaseId,
                        amount = decimal.Parse(vnp_Amount) / 100,
                        transactionId = vnp_TransactionNo,
                        responseCode = vnp_ResponseCode
                    });
                }
                else
                {
                    // Thanh toán thất bại - Cập nhật status
                    var updateDto = new UpdatePurchaseStatusDTO
                    {
                        Status = PurchaseStatus.Failed,
                        TransactionId = vnp_TransactionNo
                    };

                    await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, updateDto);

                    _logger.LogWarning($"Payment failed for Purchase {purchaseId}, ResponseCode: {vnp_ResponseCode}");

                    return Ok(new
                    {
                        success = false,
                        message = GetVnPayResponseMessage(vnp_ResponseCode),
                        purchaseId = purchaseId,
                        responseCode = vnp_ResponseCode
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay return");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("vnpay-ipn")]
        public async Task<IActionResult> VnPayIPN()
        {
            try
            {
                var vnpay = new VnPayLibrary();

                foreach (string key in Request.Query.Keys)
                {
                    if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(key, Request.Query[key]);
                    }
                }

                string vnp_HashSecret = _config["VnPay:HashSecret"];
                string vnp_SecureHash = Request.Query["vnp_SecureHash"];
                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);

                if (!checkSignature)
                {
                    return Ok(new { RspCode = "97", Message = "Invalid Signature" });
                }

                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                string vnp_TxnRef = vnpay.GetResponseData("vnp_TxnRef");
                string vnp_TransactionNo = Request.Query["vnp_TransactionNo"];

                if (!int.TryParse(vnp_TxnRef, out int purchaseId))
                {
                    return Ok(new { RspCode = "99", Message = "Invalid TxnRef" });
                }

                // Lấy Purchase
                var purchase = await _purchaseService.GetPurchaseByIdAsync(purchaseId);
                if (purchase == null)
                {
                    return Ok(new { RspCode = "01", Message = "Order Not Found" });
                }

                // Kiểm tra đơn hàng đã xử lý chưa
                if (purchase.Status == PurchaseStatus.Completed)
                {
                    return Ok(new { RspCode = "02", Message = "Order Already Confirmed" });
                }

                // Xử lý kết quả
                if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                {
                    var updateDto = new UpdatePurchaseStatusDTO
                    {
                        Status = PurchaseStatus.Completed,
                        TransactionId = vnp_TransactionNo
                    };

                    await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, updateDto);

                    // TẠO ENROLLMENT TỰ ĐỘNG (IPN callback) CHO TẤT CẢ KHÓA HỌC
                    try
                    {
                        var enrollments = await _enrollmentService.CreateEnrollmentsFromPurchaseAsync(purchaseId);
                        _logger.LogInformation($"IPN: Created {enrollments.Count()} enrollments for Purchase {purchaseId}");
                    }
                    catch (Exception enrollEx)
                    {
                        _logger.LogError(enrollEx, $"IPN: Failed to create enrollments for Purchase {purchaseId}");
                    }

                    _logger.LogInformation($"IPN: Payment confirmed for Purchase {purchaseId}");

                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
                else
                {
                    var updateDto = new UpdatePurchaseStatusDTO
                    {
                        Status = PurchaseStatus.Failed,
                        TransactionId = vnp_TransactionNo
                    };

                    await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, updateDto);

                    return Ok(new { RspCode = "00", Message = "Confirm Success" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing VNPay IPN");
                return Ok(new { RspCode = "99", Message = "Unknown error" });
            }
        }

        private string GetIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if (string.IsNullOrEmpty(ipAddress) || ipAddress == "::1")
            {
                ipAddress = "127.0.0.1";
            }
            return ipAddress;
        }

        private string GetVnPayResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Giao dịch thành công",
                "07" => "Trừ tiền thành công. Giao dịch bị nghi ngờ (liên quan tới lừa đảo, giao dịch bất thường)",
                "09" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng chưa đăng ký dịch vụ InternetBanking tại ngân hàng",
                "10" => "Giao dịch không thành công do: Khách hàng xác thực thông tin thẻ/tài khoản không đúng quá 3 lần",
                "11" => "Giao dịch không thành công do: Đã hết hạn chờ thanh toán. Xin quý khách vui lòng thực hiện lại giao dịch",
                "12" => "Giao dịch không thành công do: Thẻ/Tài khoản của khách hàng bị khóa",
                "13" => "Giao dịch không thành công do Quý khách nhập sai mật khẩu xác thực giao dịch (OTP)",
                "24" => "Giao dịch không thành công do: Khách hàng hủy giao dịch",
                "51" => "Giao dịch không thành công do: Tài khoản của quý khách không đủ số dư để thực hiện giao dịch",
                "65" => "Giao dịch không thành công do: Tài khoản của Quý khách đã vượt quá hạn mức giao dịch trong ngày",
                "75" => "Ngân hàng thanh toán đang bảo trì",
                "79" => "Giao dịch không thành công do: KH nhập sai mật khẩu thanh toán quá số lần quy định",
                _ => "Giao dịch thất bại"
            };
        }
    }
}