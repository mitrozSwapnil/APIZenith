using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class createQuotationRequest:BaseRequest
    {
        

        public string? ApplicationId { get; set; }
      

        public string? CertificateType { get; set; }

        public bool isSubmit { get; set;}

        public Dictionary<string, object> QuotationData { get; set; }
        public List<Dictionary<string, object>>? Terms { get; set; }
        //public string? ValidityofQuotation { get; set; }
        //public Dictionary<string, object> Enclouser { get; set; }
    }
}
