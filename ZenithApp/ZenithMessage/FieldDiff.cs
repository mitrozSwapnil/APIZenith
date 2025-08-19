using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class FieldDiff
    {
        public string Field { get; set; }
        public BsonValue Reviewer1Value { get; set; }
        public BsonValue Reviewer2Value { get; set; }
        public bool HasConflict { get; set; }
    }


}
