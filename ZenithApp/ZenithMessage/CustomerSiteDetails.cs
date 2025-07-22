using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class CustomerSiteDetails
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Customer_SiteId { get; set; }
        public string? Address { get; set; }
        public string? Telephone { get; set; }
        public string? Web { get; set; }
        public string? Email { get; set; }
        public string? Activity_Department { get; set; }
        public string? Manpower { get; set; }
        public string? Shift_Name { get; set; }
    }
}
