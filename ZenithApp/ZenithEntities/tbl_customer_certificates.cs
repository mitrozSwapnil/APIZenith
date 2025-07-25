using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_customer_certificates
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_Customer_Application{ get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_Certificates{ get; set; }
        public string? CertificateType{ get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? status { get; set; }

        public bool? Is_Delete{ get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
       
        public DateTime UpdatedAt { get; set; }

        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? AssignTo { get; set; }


    }
}
