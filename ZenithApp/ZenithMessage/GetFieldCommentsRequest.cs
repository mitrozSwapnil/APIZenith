namespace ZenithApp.ZenithMessage
{
    public class GetFieldCommentsRequest : BaseRequest
    {
        public string ApplicationId { get; set; }
        public string FkCertificate { get; set; }
    }
}
