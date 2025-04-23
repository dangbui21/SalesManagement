namespace PaymentService.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2
    }

    public enum PaymentEventType
    {
        Undefined = 0,  // Trạng thái chưa xác định (mặc định là 0)
        PaymentSucceeded = 1, // Thành công
        PaymentFailed = 2,    // Thất bại
        Pending = 3,         // Đang chờ
        Refunded = 4         // Hoàn tiền
    }
}