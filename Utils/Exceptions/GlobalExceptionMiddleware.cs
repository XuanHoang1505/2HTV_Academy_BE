using System.Net;
using System.Text.Json;

namespace App.Utils.Exceptions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Global exception caught");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.Clear();
            response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.";
            var errorCode = ErrorCode.InternalServerError.ToString();

            if (exception is AppException appEx)
            {
                statusCode = GetStatusCode(appEx.ErrorCode);
                message = appEx.Message;
                errorCode = appEx.ErrorCode.ToString();
            }

            response.StatusCode = statusCode;

            var errorResponse = new
            {
                success = false,
                message,
                errorCode
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
        }

        private static int GetStatusCode(ErrorCode errorCode)
        {
            return errorCode switch
            {
                ErrorCode.InvalidLogin        => (int)HttpStatusCode.Unauthorized, // 401
                ErrorCode.EmailAlreadyExists  => (int)HttpStatusCode.Conflict,     // 409
                ErrorCode.UserNotFound        => (int)HttpStatusCode.NotFound,     // 404
                ErrorCode.CourseNotFound     => (int)HttpStatusCode.NotFound,     // 404
                ErrorCode.PurchaseNotFound   => (int)HttpStatusCode.NotFound,     // 404
                ErrorCode.InvalidInput        => (int)HttpStatusCode.BadRequest,   // 400
                ErrorCode.AccountLocked       => (int)HttpStatusCode.Forbidden,    // 403
                _                             => (int)HttpStatusCode.InternalServerError
            };
        }
    }
}
