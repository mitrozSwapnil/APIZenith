using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class ReviewerThreatList
    {
        public string? Name { get; set; }
        public string? Comment { get; set; }
        public bool? Applicable { get; set; } = false;
    }
}
