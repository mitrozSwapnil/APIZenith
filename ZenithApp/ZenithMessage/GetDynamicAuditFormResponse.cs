using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class GetDynamicAuditFormResponse:BaseResponse
    {
        public List<tbl_dynamic_audit_template> Data { get; set; } = new();
    }
}
