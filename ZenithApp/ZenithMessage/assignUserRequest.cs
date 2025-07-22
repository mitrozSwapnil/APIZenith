namespace ZenithApp.ZenithMessage
{
    public class assignUserRequest : BaseRequest
    {
        public string ApplicationId { get; set; }
        public string UserId { get; set; }
        public string Certification_Id { get; set; }
    }
}
