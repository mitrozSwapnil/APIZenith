namespace ZenithApp.ZenithMessage
{
    public class SaveReviewerApplicationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public System.Net.HttpStatusCode HttpStatusCode { get; set; }
        public int ResponseCode { get; set; }
        public int NewVersion { get; set; }
    }
}
