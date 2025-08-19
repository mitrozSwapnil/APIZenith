namespace ZenithApp.ZenithMessage
{
    public class addICMEDApplicationRequest : BaseRequest
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public DateTime? ApplicationReviewDate { get; set; }

        public string? ActiveReviwer { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Certification_Name { get; set; }  // Should match model
        public string? Fk_Certificate { get; set; }
        public string? Audit_Type { get; set; }
        public string? AssignTo { get; set; }

        public string? Scop_of_Certification { get; set; }
        public string? remark { get; set; }

        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public bool? IsInterpreter { get; set; }
        public bool? IsMultisitesampling { get; set; }

        public int? Total_site { get; set; }
        public List<LabelValue> Sample_Site { get; set; } = new();
        public List<LabelValue> Shift_Details { get; set; } = new();
        public string? IsFinalSubmit { get; set; }
        public string? Fk_UserId { get; set; }
        public int? ActiveState { get; set; } = 1;

        public List<TechnicalAreasList> Technical_Areas { get; set; } = new();
        public List<AccreditationsList> Accreditations { get; set; } = new();

        public List<ReviewerSiteDetails> CustomerSites { get; set; } = new();
        public List<ReviewerKeyPersonnelList> KeyPersonnels { get; set; } = new();
        public List<ReviewerAuditMandaysList> MandaysLists { get; set; } = new();

        public List<ReviewerThreatList> ThreatLists { get; set; } = new();
        public List<ReviewerRemarkList> RemarkLists { get; set; } = new();
    }


}
