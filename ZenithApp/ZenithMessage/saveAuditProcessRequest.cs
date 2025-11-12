namespace ZenithApp.ZenithMessage
{
    public class saveAuditProcessRequest: BaseRequest
    {
        public string AuditId { get; set; }
        public string StepObjectId { get; set; }
        public string StepName { get; set; }
        public string url { get; set; }
        public string ApplicationId { get; set; }
        public string CertificateId { get; set; }
    }
}
