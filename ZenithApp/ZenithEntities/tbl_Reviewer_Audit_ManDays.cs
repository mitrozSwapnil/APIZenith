using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_Reviewer_Audit_ManDays
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_ApplicationId { get; set; }

        public string? Audit_Type { get; set; }

        public string? ActivityName { get; set; }

        public decimal Man_Days { get; set; }

        public decimal Additional_Mandays { get; set; }

        [BsonElement("OnSite_manDays_Stage_1")]
        public decimal OnSite_manDays_Stage_1 { get; set; }

        [BsonElement("OnSite_manDays_Stage_2")]
        public decimal OnSite_manDays_Stage_2 { get; set; }

        [BsonElement("OfSite_manDays_Stage_1")]
        public decimal OfSite_manDays_Stage_1 { get; set; }

        [BsonElement("OfSite_manDays_Stage_2")]
        public decimal OfSite_manDays_Stage_2 { get; set; }

        public string? Note { get; set; }

        public string? Stage { get; set; }

        [BsonElement("Standard_Guidline")]
        public string? Standard_Guidline { get; set; }

        public string? Technical_Review { get; set; }

        public string? Comment { get; set; }

        public string? AssesmentComment { get; set; }

    }
}
