using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_quoatation
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string ApplicationId { get; set; }
        public string QuotationId { get; set; }
        public string Currency { get; set; }

        public string CertificateType { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public BsonDocument QuotationData { get; set; }
    }
}
