namespace ZenithApp.ZenithMessage
{
    public class getAuditNominationResponse : BaseResponse
    {
        public List<AuditNominationDetails> Data { get; set; } = new();
    }

    public class AuditNominationDetails
    {
        public string Id { get; set; }
        public string AuditId { get; set; }
        public string ApplicationId { get; set; }
        public string CertificationId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public double? TotalAuditTime { get; set; }
        public double? OnsiteAuditTime { get; set; }
        public double? OffsiteActivityTime { get; set; }

        public string AdditionalInfo { get; set; }
        public List<NominatedTeamResponse> TeamDetails { get; set; } = new();
    }

    public class NominatedTeamResponse
    {
        public string UserId { get; set; }  // optional - if you want to track each member
        public string Name { get; set; }
        public string Role { get; set; }
        public string TechnicalArea { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }


}
