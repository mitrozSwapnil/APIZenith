using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class userDropdownResponse : BaseResponse
    {
        public List<UserDropdownDto> Data { get; set; } = new();

    }
}