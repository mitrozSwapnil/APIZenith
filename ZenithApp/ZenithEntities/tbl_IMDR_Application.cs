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

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_Certificate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AssignTo { get; set; }
        public string? Scop_of_Certification { get; set; }
        public string? DeviceMasterfile { get; set; }
        public bool? Availbility_of_TechnicalAreas { get; set; }
        public bool? Availbility_of_Auditor { get; set; }
        public string? Audit_Lang { get; set; }
        public int? ActiveState { get; set; } = 1;

        public bool? IsInterpreter { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedAt { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Status { get; set; }
        public string? ActiveReviwer { get; set; }
        public bool? IsDelete { get; set; }
        public bool? IsFinalSubmit { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_UserId { get; set; }
        public List<TechnicalAreasList> Technical_Areas { get; set; } = new List<TechnicalAreasList>();
        public List<ReviewerSiteDetails> CustomerSites { get; set; } = new List<ReviewerSiteDetails>();
        public List<KeyPersonnelList> KeyPersonnels { get; set; } = new List<KeyPersonnelList>();
        public List<ImdrManDays> imdrManDays { get; set; } = new List<ImdrManDays>();
        public List<ReviewerKeyPersonnelList> reviewerKeyPersonnel { get; set; } = new List<ReviewerKeyPersonnelList>();
        public List<ReviewerThreatList> ReviewerThreatList { get; set; } = new List<ReviewerThreatList>();
        public List<ReviewerRemarkList> ReviewerRemarkList { get; set; } = new List<ReviewerRemarkList>();
        public List<MDRAuditList> mdrauditLists { get; set; } = new List<MDRAuditList>();
    }
}
