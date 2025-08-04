using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class getReviewerApplicationResponse: BaseResponse
    {
        //public List<ReviewerApplicationData> Data { get; set; }
        public object Data { get; set; }
        public string CertificateName { get; set; }

        public string AssigntToName { get; set; }
        public string statusName { get; set; }

    }
}
