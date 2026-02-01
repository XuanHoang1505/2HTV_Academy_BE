namespace App.Utils.Exceptions
{
    public enum ErrorCode
    {
        // 🔹 Lỗi xác thực (Authentication Errors)
        InvalidLogin = 1001,
        UnauthorizedAccess = 1002,
        Forbidden = 1003,
        InvalidData = 1004,

        // 🔹 Lỗi tài khoản và người dùng (User & Account Errors)
        EmailAlreadyExists = 2001,
        PhoneNumberAlreadyExists = 2002,
        UserNotFound = 2003,
        AccountLocked = 2004,
        ReviewNotFound = 2005,
        AccountNotVerified = 2006,
        IncorrectOldPassword = 2007,
        VipTypeNotFound = 2008,
        EnrollmentNotFound = 2009,
        VipAlreadyExists = 2010,
        CommentNotFound = 2011,

        // 🔹 Lỗi hệ thống và máy chủ (System Errors)
        InternalServerError = 5000,
        ServiceUnavailable = 5001,

        // 🔹 Lỗi dữ liệu không hợp lệ (Validation Errors)
        InvalidEmailFormat = 3001,
        InvalidInput = 3002,
        PasswordTooWeak = 3003,
        InvalidUsername = 3004,
        InvalidDate = 3005,
        MissingRequiredField = 3006, // Thiếu dữ liệu bắt buộc
        InvalidSlugFormat = 3007,    // Slug không hợp lệ

        // 🔹 Lỗi tài nguyên không tìm thấy (Resource Not Found Errors)
        ResourceNotFound = 4001,
        RoleNotFound = 4003,
        ArticleNotFound = 4004,
        TagNotFound = 4005,
        NotificationNotFound = 4006,
        FileNotFound = 4007,

        // 🔹 Lỗi xung đột dữ liệu (Conflict Errors)
        ConflictError = 409,
        ReviewAlreadyExists = 4091,
        VerificationTokenInvalidOrExpired = 4092,
        CategoryNotFound = 6001,
        CourseAlreadyEnrolled= 6002,

        CartNotFound = 6006,
        CartItemNotFound = 6007,
        CourseAlreadyInCart = 6008,
        CourseNotFound = 6009,
        EmptyCart = 6010,
        SlugAlreadyExists = 6011,

        CategorySlugAlreadyExists = 6002,
        ParentCategoryNotFound = 6003,
        ArticleSlugAlreadyExists = 6004,
        TagAlreadyExists = 6005,

        // 🔹 Lỗi upload/file
        FileUploadFailed = 7001,
        FileTypeNotAllowed = 7002,
        FileTooLarge = 7003,

        // 🔹 Lỗi thanh toán / subscription
        PaymentFailed = 8001,
        SubscriptionExpired = 8002,
        SubscriptionNotFound = 8003,
        InsufficientBalance = 8004,

        // 🔹 Lỗi like/vote
        AlreadyLiked = 9001,
        AlreadyDisliked = 9002,
        VoteNotFound = 9003,

        PurchaseNotFound = 9004,

        AlreadyPurchase = 9005,
        AlreadyReview = 9006,
        NotFound = 404,
        BadRequest = 400,
        Unauthorized = 401,
    }
}
