namespace ZenithApp.ZenithMessage
{
    public class AddMasterUsersRequest : BaseRequest
    {
        public string UserId { get; set; }
        public string CertificateId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string Password { get; set; }
    }
}
