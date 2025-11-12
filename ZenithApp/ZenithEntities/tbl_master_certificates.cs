using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_master_certificates
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? Certificate_Name { get; set; }

        public bool? Is_Delete { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreatedAt { get; set; }
        public string? CertificationType { get; set; }
        // public List<string>? SubType { get; set; }

        [BsonElement("SubType")]
        public List<SubTypeModel> SubType { get; set; }
    }
    public class SubTypeModel
    {
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        public string Scheme { get; set; }

        public string Annexure { get; set; }
    }
}
