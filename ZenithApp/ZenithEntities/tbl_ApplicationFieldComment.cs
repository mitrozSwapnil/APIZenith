using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZenithApp.ZenithEntities
{
    public class tbl_ApplicationFieldComment
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string CertificationName { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string ApplicationId { get; set; } //name uniqe name ZAQPL-001

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_User { get; set; }

        public string FieldName { get; set; }
        public string FieldUserType{ get; set; }

        public string CommentText { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatedBy { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedOn { get; set; }
    }
}
