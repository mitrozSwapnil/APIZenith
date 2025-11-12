using ZenithApp.ZenithMessage;

namespace ZenithApp
{
    public class GetAuditDetailsResponse:BaseResponse
    {
        public List<SidePannaleData> Data { get; set; }
    }
    public class SidePannaleData
    {
        public string CompanyName { get; set; }
        public string CustomerApplicationId { get; set; }
        public string AuditId { get; set; }
        public string FileNumber { get; set; }
        public string Address { get; set; }
        public string fkCertificate { get; set; }
        public DateTime? AuditStartDate { get; set; }
        public DateTime? AuditEndDate { get; set; }
        public string AddministrationUrl { get; set; }
        public string TechnicalReviewUrl { get; set; }
        public List<TeamMember> TeamDetails { get; set; }

    }
    public class TeamMember
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
