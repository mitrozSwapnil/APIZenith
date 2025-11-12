namespace ZenithApp.ZenithMessage
{
    public class getAuditNominationRequest : BaseRequest
    {
        public string ApplicationId { get; set; }
        public string CertificationId { get; set; }
        public string AuditId { get; set; }
    }
}
