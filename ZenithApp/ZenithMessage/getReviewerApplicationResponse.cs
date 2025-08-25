using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class getReviewerApplicationResponse: BaseResponse
    {
        public object Data { get; set; }
        public List<tbl_ApplicationFieldComment> Comments { get; set; } = new();
        public string CertificateName { get; set; }
        public string AssigntToName { get; set; }
        public string statusName { get; set; }
        public string AssignRole { get; set; }

    }
}
