namespace ZenithApp.ZenithMessage
{
    public class getmandaysbyapplicationIdRequest:BaseRequest
    {
        public string? ApplicationId { get; set; }
        public string CertificateTypeId { get; set; }
    }
}
