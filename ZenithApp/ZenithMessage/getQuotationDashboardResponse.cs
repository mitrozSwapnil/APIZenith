namespace ZenithApp.ZenithMessage
{
    public class getQuotationDashboardResponse:BaseResponse
    {
        public List<QuotationDashboardData> Data { get; set; }
        public PageinationDto Pagination { get; set; }
    }
}
