using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_Reviewer_Personnel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string? Fk_site { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string Fk_ApplicationId { get; set; }
        public string? Department { get; set; }
        public string? Comment { get; set; }
        public int? No_of_Effective_Personnel{ get; set; }
    }
}
