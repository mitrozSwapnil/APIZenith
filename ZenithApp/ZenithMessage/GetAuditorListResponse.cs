namespace ZenithApp.ZenithMessage
{
    public class GetAuditorListResponse : BaseResponse
    {
        public List<AuditorDetails> Data { get; set; } = new();
    }
    public class AuditorDetails
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string TechnicalArea { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}
