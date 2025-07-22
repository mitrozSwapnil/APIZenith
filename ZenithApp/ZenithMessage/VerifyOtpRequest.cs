namespace ZenithApp.ZenithMessage
{
    public class VerifyOtpRequest : BaseRequest
    {
        public string? userId { get; set; }
        public string? otp { get; set; }
    }
}
