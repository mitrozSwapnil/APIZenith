using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class GetMasterUsersResponse:BaseResponse
    {
        public List<tbl_user> Data { get; set; }
    }
}
