using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_ApplicationReview
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        // e.g. "ISO", "FSSC", "ICMED", "ICMED_PLUS", "IMDR"
        public string CertificationName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_ApplicationId { get; set; } // -> tbl_ISO_Application.Id (or FSSC, etc.)

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReviewerId { get; set; }       // -> tbl_user.Id

        // The reviewer’s form data (only the fields they edit/save).
        // Keep it schema-lite so the same collection works for all certificates.
        public BsonDocument Fields { get; set; } = new BsonDocument();

        public bool IsFinalSubmit { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? SubmitDate { get; set; }

        // Optimistic concurrency (prevent overwriting stale drafts)
        public int Version { get; set; } = 1;

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime CreatedAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime UpdatedAt { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UpdatedBy { get; set; }
    }
}
