using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class tbl_audit_administration_technical
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string AuditId { get; set; }

        public string Type { get; set; }  // "technical"
        public List<TechnicalItem> Technical { get; set; }
        public string AdditionalComments { get; set; }
        public string ReviewedBy { get; set; }

        public bool IsDelete { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
    public class TechnicalItem
    {
        public string Copy { get; set; }                  // e.g. "auditPlan"
        public string Compliant { get; set; }
        public string ReviewerComments { get; set; }
        public string LeadAuditorResponse { get; set; }
        public string ReviewerAcceptance { get; set; }
    }
}
