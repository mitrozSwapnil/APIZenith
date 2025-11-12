namespace ZenithApp.ZenithMessage
{
    public class userDropdownRequest: BaseRequest
    {
        public string type { get; set; } // Type of dropdown, e.g., "user", "admin", etc.
        public string? CertificationName { get; set; } // Type of dropdown, e.g., "user", "admin", etc.
    }
}
