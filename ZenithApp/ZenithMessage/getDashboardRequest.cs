namespace ZenithApp.ZenithMessage
{
    public class getDashboardRequest : BaseRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }

    }
}
