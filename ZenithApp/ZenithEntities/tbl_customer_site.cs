using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_customer_site
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_Customer_Application { get; set; }

        public string? Address { get; set; }

        public string? Telephone { get; set; }

        public string? Web { get; set; }

        public string? Email { get; set; }

        public string? Activity_Department { get; set; }

        public string? Manpower { get; set; }

        public string? Shift_Name { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_User { get; set; }
        public bool? Is_Delete { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedAt { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime UpdatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }


    }
}
