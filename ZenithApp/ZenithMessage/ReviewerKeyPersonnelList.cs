using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class ReviewerKeyPersonnelList
    {
       
        public string ActivityName { get; set; }
        public string? PersonnelByClient { get; set; }
        public string EffectivePersonnel { get; set; }
        public string Comment { get; set; }
        public string AdditionalComments { get; set; }
        public string Fk_site { get; set; }
    }
}
