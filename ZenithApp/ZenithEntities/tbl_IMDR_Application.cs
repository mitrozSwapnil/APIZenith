using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithEntities
{
    public class tbl_IMDR_Application
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? ApplicationId { get; set; }
        public DateTime? Application_Received_date { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_Certificate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssignTo { get; set; }



        public string? Audit_Type { get; set; }
        public string? Scop_of_Certification { get; set; }
        public string? masterfile { get; set; }
        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public int? ActiveState { get; set; } = 1;

        public bool? IsInterpreter { get; set; }
        public bool? IsMultisitesampling { get; set; }
        public int? Total_site { get; set; }
        public List<LabelValue> Sample_Site { get; set; }
        public List<LabelValue> Shift_Details { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ApplicationReviewDate { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Status { get; set; }
        public string? Application_Status { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsFinalSubmit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_UserId { get; set; }
        public List<TechnicalAreasList> Technical_Areas { get; set; } = new List<TechnicalAreasList>();
        public List<AccreditationsList> Accreditations { get; set; } = new List<AccreditationsList>();

        public List<ReviewerSiteDetails> CustomerSites { get; set; } = new List<ReviewerSiteDetails>();
        public List<KeyPersonnelList> KeyPersonnels { get; set; } = new List<KeyPersonnelList>();
        public List<ReviewerKeyPersonnelList> reviewerKeyPersonnel { get; set; } = new List<ReviewerKeyPersonnelList>();
        public List<ReviewerAuditMandaysList> MandaysLists { get; set; } = new List<ReviewerAuditMandaysList>();
        public List<ThreatList> ThreatLists { get; set; } = new List<ThreatList>();
        public List<ReviewerThreatList> ReviewerThreatList { get; set; } = new List<ReviewerThreatList>();
        public List<ReviewerRemarkList> ReviewerRemarkList { get; set; } = new List<ReviewerRemarkList>();
        //public List<RemarkList> RemarkLists { get; set; } = new List<RemarkList>();
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
