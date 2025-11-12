using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class GetAuditProcessResponse:BaseResponse
    {
        public List<tbl_AuditProcess> Data { get; set; }
    }
}
