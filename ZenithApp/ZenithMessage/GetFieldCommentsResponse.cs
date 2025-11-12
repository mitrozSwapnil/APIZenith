using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class GetFieldCommentsResponse : BaseResponse
    {
        public List<tbl_ApplicationFieldComment> Comments { get; set; }
    }
}
