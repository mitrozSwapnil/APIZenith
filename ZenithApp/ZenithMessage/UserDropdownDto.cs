using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class UserDropdownDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        // For certificate types that include subtypes
        public List<SubTypeDto>? SubTypes { get; set; }
    }
    public class SubTypeDto
    {
        
        public string Id { get; set; }
        public string Scheme { get; set; }
        public string Annexure { get; set; }
    }
}
