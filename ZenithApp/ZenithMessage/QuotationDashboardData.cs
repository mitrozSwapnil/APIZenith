namespace ZenithApp.ZenithMessage
{
    public class QuotationDashboardData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? QuotationId { get; set; }
        public string? ApplicationName { get; set; }
        public string? CompanyName { get; set; }
        //public bool? IsFinal { get; set; }
        public string? Certification_Name { get; set; }
        public string? Certification_Id { get; set; }
     //   public string? Status { get; set; }
        public long? CertificateCount { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public string? AssignedUserName { get; set; }
    }
}
