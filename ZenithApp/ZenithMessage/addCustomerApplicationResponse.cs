using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class addCustomerApplicationResponse : BaseResponse
    {
        public tbl_customer_application Data { get; set; }
    }
}
