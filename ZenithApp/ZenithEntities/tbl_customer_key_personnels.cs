using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_customer_key_personnels
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_Customer_Application { get; set; }

        public string? FullName { get; set; }

        public string? Department { get; set; }

        public string? EmailId { get; set; }
        public string? Contact { get; set; }

        public string? Contact_No { get; set; }
        public string? TypeOfPersonnel { get; set; }  // For "Top Management / Authorize Representative"

        public bool? Is_Delete { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        // Uncomment if you need to reference another collection

        //[BsonRepresentation(BsonType.ObjectId)]
        //public string? FK_IMdR_Application { get; set; }
    }
}
