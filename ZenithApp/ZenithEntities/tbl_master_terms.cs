using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_master_terms
    {


        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string term { get; set; }

        public bool isDelete { get; set; }
      
        public DateTime CreatedOn { get; set; }
    }
}
