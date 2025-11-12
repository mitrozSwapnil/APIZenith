using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ZenithApp.ZenithEntities
{
    public class StepTemplateModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string AttachmentType { get; set; }
        public string FormName { get; set; }
        public string UpdatedBy { get; set; }
        public string file { get; set; }
        public string Status { get; set; }
        public Boolean isApproval { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
