namespace ZenithApp.ZenithMessage
{
    public class assignUserRequest : BaseRequest
    {
        public string ApplicationId { get; set; }
        public string TrineeId { get; set; }
        public string ReviewerId { get; set; }
        public string Certification_Id { get; set; }
    }
}
