using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithEntities
{
    public class tbl_user
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName{ get; set; }
        public string? Password{ get; set; }
        public string? Type{ get; set; }
        public string? EmailId{ get; set; }
        public List<string>? Department { get; set; }
        public string? ContactNo{ get; set; }
        public string? Address{ get; set; }
        public string? Gender { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_RoleID { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Fk_Otp{ get; set; }
        public int? IsDelete{ get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt{ get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy{ get; set; }
    }
}
