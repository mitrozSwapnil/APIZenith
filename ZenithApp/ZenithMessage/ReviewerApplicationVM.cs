using MongoDB.Bson;
using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class ReviewerApplicationVM<TBase>
    {
        public TBase Base { get; set; }                                // typed base doc (e.g., tbl_ISO_Application)
        public BsonDocument YourDraft { get; set; }                    // your saved fields
        public BsonDocument OtherDraft { get; set; }                   // other reviewer’s fields
        public List<FieldDiff> Diffs { get; set; } = new();
        public List<tbl_ApplicationFieldComment> Comments { get; set; } = new();
        public List<tbl_ApplicationFieldHistory> History { get; set; } = new();
        public string CertificateName { get; set; }                    // for convenience
        public string StatusName { get; set; }                         // for convenience
    }
}
