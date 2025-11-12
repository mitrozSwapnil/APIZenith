namespace ZenithApp.ZenithMessage
{
    public class getAuditResponse: BaseResponse
    {
        public List<AuditDashboardData> Data { get; set; }
        public PageinationDto Pagination { get; set; }
    }
}
