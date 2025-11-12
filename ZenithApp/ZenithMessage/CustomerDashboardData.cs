namespace ZenithApp.ZenithMessage
{
    public class CustomerDashboardData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? sub_applicationId { get; set; }
        public string? ApplicationName { get; set; }
        public string? CompanyName { get; set; }
        public bool? IsFinal { get; set; }
        public string? Certification_Name { get; set; }
        public string? Certification_Id{ get; set; }
        public string? Status { get; set; }
        public long? CertificateCount { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public DateTime? TargetDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? AssignedUserName { get; set; }

        public int totalComments { get; set; }
        public string TraineeName { get; set; }
        public string ReviewerName { get; set; }




    }
}
