using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_outsource_process
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public int Application_Id { get; set; }

        public string? Process { get; set; }

        [BsonElement("Contractor_Supplier")]
        public string? Contractor_Supplier { get; set; }

        public string? Location { get; set; }

        public string? Type_Level { get; set; }
    }
}
