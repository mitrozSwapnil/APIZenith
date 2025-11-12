using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class tbl_AssignAudit
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string AuditId { get; set; }
        public string ApplicationId { get; set; }
        public string CertificationId { get; set; }

        public DateTime StartDate { get; set; }= DateTime.UtcNow;
        public DateTime EndDate { get; set; }= DateTime.UtcNow;
        public string StartTime { get; set; } 
        public string EndTime { get; set; }
        public DateTime UpdatedAt { get; set; }= DateTime.UtcNow;

        public double TotalAuditTime { get; set; }
        public double OnsiteAuditTime { get; set; }
        public double OffsiteActivityTime { get; set; }

        public string AdditionalInfo { get; set; }

        public List<NominatedTeam> TeamDetails { get; set; } = new List<NominatedTeam>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class NominatedTeam
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string TechnicalArea { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
    }
}

