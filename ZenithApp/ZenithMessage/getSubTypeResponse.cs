using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class getSubTypeResponse : BaseResponse 
    {
        
        public List<SubTypeModel> SubTypes { get; set; }
    }
}
