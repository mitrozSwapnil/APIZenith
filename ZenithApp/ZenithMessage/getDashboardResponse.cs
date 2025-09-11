using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class getDashboardResponse : BaseResponse
    {
        public List<CustomerDashboardData> Data { get; set; }
        public PageinationDto Pagination { get; set; }
        public PannelDto Pannale { get; set; }

    }
}
