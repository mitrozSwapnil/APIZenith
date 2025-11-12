namespace ZenithApp.ZenithMessage
{
    public class SaveUpdatesRequest:BaseRequest
    {
        public string AuditId { get; set; }
        public string Updates { get; set; }
        public string ApplicationId { get; set; }
    }
}
