namespace ZenithApp.ZenithMessage
{
    public class statusRequest:BaseRequest
    {
        public string? applicationId { get; set; }
        public string? status { get; set; }
    }
}
