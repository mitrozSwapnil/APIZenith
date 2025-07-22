namespace ZenithApp.ZenithMessage
{
    public class getDashboardResponse : BaseResponse
    {
        public List<CustomerDashboardData> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
