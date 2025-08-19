using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_ApplicationFieldHistory
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CertificationName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_ApplicationId { get; set; }

        public string FieldName { get; set; }
        public string ReviwerType { get; set; }

        // Store as BsonValue to support any shape (string, number, array, object)
        public BsonValue OldValue { get; set; }
        public BsonValue NewValue { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ChangedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime ChangedOn { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CommentId { get; set; } // optional link to comment if provided
    }
}
