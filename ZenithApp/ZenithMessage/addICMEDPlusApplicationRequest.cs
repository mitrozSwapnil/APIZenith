namespace ZenithApp.ZenithMessage
{
    public class addICMEDPlusApplicationRequest
    {
        public string? Id { get; set; }
        public string? ApplicationType { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public DateTime? ApplicationReviewDate { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? Fk_Certificate { get; set; }
        public string? AssignTo { get; set; }
        public string? Seasonality_Factor { get; set; }
        public string? AnyAllergens { get; set; }
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

        //new properties for FSSC application
        public List<stage1AndStage2Audit> auditLists { get; set; } = new List<stage1AndStage2Audit>();
        public List<ServeillannceAudit> serveillannceAuditLists { get; set; } = new List<ServeillannceAudit>();
        public List<ReCertificationAudit> reCertificationAudits { get; set; } = new List<ReCertificationAudit>();
        public List<TransferAudit> transferAudits { get; set; } = new List<TransferAudit>();
        public List<SpecialAudit> specialAudits { get; set; } = new List<SpecialAudit>();
        public List<ProductCategoryAndSubCategoryList> productCategoryAndSubs { get; set; } = new List<ProductCategoryAndSubCategoryList>();
        public List<HACCPList> hACCPLists { get; set; } = new List<HACCPList>();
        public List<StandardsList> standardsLists { get; set; } = new List<StandardsList>();
        public List<CategoryList> categoryLists { get; set; } = new List<CategoryList>();
        public List<SubCategoryList> subCategoryLists { get; set; } = new List<SubCategoryList>();
    }
}
