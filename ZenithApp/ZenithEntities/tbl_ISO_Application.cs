using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using ZenithApp.ZenithMessage;

namespace ZenithApp.ZenithEntities
{
    public class tbl_ISO_Application
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
        public DateTime? CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? Status { get; set; }
        public string? Application_Status { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsFinalSubmit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_UserId { get; set; }
        public List<string> Technical_Areas { get; set; } = new List<string>();
        public List<string> Accreditations { get; set; } = new List<string>();

        public List<CustomerSiteDetails> CustomerSites { get; set; } = new List<CustomerSiteDetails>();
        public List<KeyPersonnelList> KeyPersonnels { get; set; } = new List<KeyPersonnelList>();
        public List<ReviewerAuditMandaysList> MandaysLists { get; set; } = new List<ReviewerAuditMandaysList>();
        public List<ThreatList> ThreatLists { get; set; } = new List<ThreatList>();
        public List<RemarkList> RemarkLists { get; set; } = new List<RemarkList>();


    }
}
