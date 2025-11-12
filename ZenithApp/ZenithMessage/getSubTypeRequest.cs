namespace ZenithApp.ZenithMessage
{
    public class getSubTypeRequest: BaseRequest
    {
        public string id { get; set; } // Type of dropdown, e.g., "user", "admin", etc.
    }
}
