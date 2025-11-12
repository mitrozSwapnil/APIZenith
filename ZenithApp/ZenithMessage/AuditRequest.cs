namespace ZenithApp.ZenithMessage
{
    public class AuditRequest: BaseRequest
    {
        public string sub_applicationId { get; set; }
        public string certificateid { get; set; }
        public string ApplicationId { get; set; }
        public string quotationId { get; set; }
        public string fileNumber { get; set; }

    }
}
