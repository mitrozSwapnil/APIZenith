namespace ZenithApp.ZenithMessage
{
    public class ReviewerApplicationData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? Certification_Name { get; set; }
        public string? Audit_Type { get; set; }
        public string? Scop_Of_Certification { get; set; }
        public List<TechnicalAreasList> Technical_Areas { get; set; }
        public List<AccreditationsList> Accreditations { get; set; }
        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public string? status { get; set; }
        public string? AssignUser { get; set; }
        public bool? Is_Interpreter { get; set; }
        public bool? Is_Multisite_Sampling { get; set; }
        public int? Total_Site { get; set; }
        public List<LabelValue> Sample_Site { get; set; }
        public List<LabelValue> Shift_Details { get; set; }
       public List<ReviewerSiteDetails> CustomerSites { get; set; } = new List<ReviewerSiteDetails>();
        public List<KeyPersonnelList> KeyPersonnels { get; set; } = new List<KeyPersonnelList>();
        public List<ReviewerAuditMandaysList> MandaysLists { get; set; } = new List<ReviewerAuditMandaysList>();
        public List<ThreatList> ThreatLists { get; set; } = new List<ThreatList>();
        public List<RemarkList> RemarkLists { get; set; } = new List<RemarkList>();
    }
}
