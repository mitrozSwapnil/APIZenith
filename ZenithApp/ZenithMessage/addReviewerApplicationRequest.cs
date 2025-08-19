namespace ZenithApp.ZenithMessage
{
    public class addReviewerApplicationRequest:BaseRequest
    {
        public string? Id { get; set; }
        public string? ApplicationType { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? Fk_Certificate { get; set; }
        public string? ActiveReviwer { get; set; }
        public string? AssignTo { get; set; }
        public string? Audit_Type { get; set; }
        public string? Scop_of_Certification { get; set; }
        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public bool? IsInterpreter { get; set; }
        public bool? IsMultisitesampling { get; set; }
        public int? Total_site { get; set; }
        public List<LabelValue> Sample_Site { get; set; }
        public List<LabelValue> Shift_Details { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? Application_Status { get; set; }
        public bool? IsDelete { get; set; }
        public string? IsFinalSubmit { get; set; }
        public string? Fk_UserId { get; set; }
        public int? ActiveState { get; set; }
        public List<TechnicalAreasList> Technical_Areas { get; set; }
        public List<AccreditationsList> Accreditations { get; set; }
        public List<ReviewerSiteDetails> CustomerSites { get; set; }
        public List<ReviewerKeyPersonnelList> KeyPersonnels { get; set; }
        public List<ReviewerAuditMandaysList> MandaysLists { get; set; }
        public List<ReviewerThreatList> ThreatLists { get; set; }
        public List<ReviewerRemarkList> RemarkLists { get; set; }
    }
}
