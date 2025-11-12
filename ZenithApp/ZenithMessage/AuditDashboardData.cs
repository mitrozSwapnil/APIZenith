namespace ZenithApp.ZenithMessage
{
    public class AuditDashboardData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? ApplicationNumber { get; set; }
        public string? sub_applicationId { get; set; }
        public string? QuotationId { get; set; }
        public string? QuotationNumber { get; set; }
        public string? ApplicationName { get; set; }
        public string? FileNumber { get; set; }
        public string? CompanyName { get; set; }
        public string? Certification_Name { get; set; }
        public string? Certification_Id { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public string? AssignedUserName { get; set; }
        public string? status { get; set; }
        public bool? IsAuditNominationDone { get; set; }
    }
}
