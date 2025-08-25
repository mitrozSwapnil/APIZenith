namespace ZenithApp.ZenithMessage
{
    public class getApplicationHistoryRequest:BaseRequest
    {
        public string applicationId { get; set; }
        public string CertificationName { get; set; }
        public string userId { get; set; }
    }
}
