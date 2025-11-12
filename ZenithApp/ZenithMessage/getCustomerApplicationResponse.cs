using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class getCustomerApplicationResponse : BaseResponse
    {
        public List<tbl_customer_application> Data { get; set; }
    }
}
