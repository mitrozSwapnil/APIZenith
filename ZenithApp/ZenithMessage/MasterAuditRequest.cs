namespace ZenithApp.ZenithMessage
{
    public class MasterAuditRequest : BaseRequest
    {
        public string? AuditId { get; set; }
        public string? AuditName { get; set; }
    }
}
