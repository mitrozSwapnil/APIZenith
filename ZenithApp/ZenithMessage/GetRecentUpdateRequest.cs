namespace ZenithApp.ZenithMessage
{
    public class GetRecentUpdateRequest:BaseRequest
    {
        public string AuditId { get; set; }
        public string ApplicationId { get; set; }  // optional
        public string CertificateId { get; set; }  // optional
    }
}
