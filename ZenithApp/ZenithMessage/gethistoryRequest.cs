namespace ZenithApp.ZenithMessage
{
    public class gethistoryRequest : BaseRequest
    {
        public string? ApplicationId { get; set; } //example: "ZQPL-2023-0001"
        public string? Fk_User { get; set; } //example: "ZQPL-2023-0001"
        public string? CertificationName { get; set; } //example: "ZQPL-2023-0001"
    }
}
