namespace ZenithApp.ZenithMessage
{
    public class addIMDRApplicationRequest:BaseRequest
    {

        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? ActiveReviwer { get; set; }
        public string? Fk_Certificate { get; set; }
        public string? AssignTo { get; set; }
        public string? Scop_of_Certification { get; set; }
        public string? DeviceMasterfile { get; set; }
        public string? IsFinalSubmit { get; set; }
        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public int? ActiveState { get; set; } = 1;
        public bool? IsInterpreter { get; set; }
        public string? Fk_UserId { get; set; }

        public List<TechnicalAreasList> Technical_Areas { get; set; } = new();
        public List<ReviewerSiteDetails> CustomerSites { get; set; } = new();
        public List<KeyPersonnelList> KeyPersonnels { get; set; } = new();
        public List<ImdrManDays> imdrManDays { get; set; } = new();
        public List<ReviewerKeyPersonnelList> reviewerKeyPersonnel { get; set; } = new();
        public List<ReviewerThreatList> ReviewerThreatList { get; set; } = new();
        public List<ReviewerRemarkList> ReviewerRemarkList { get; set; } = new();
        public List<MDRAuditList> mdrauditLists { get; set; } = new();

    }
}
