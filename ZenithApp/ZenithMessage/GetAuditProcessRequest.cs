namespace ZenithApp.ZenithMessage
{
    public class GetAuditProcessRequest:BaseRequest
    {
        public string AuditId { get; set; }
        public string StepObjectId { get; set; }
    }
}
