using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class createQuotationRequest:BaseRequest
    {
        

        public string? ApplicationId { get; set; }
      

        public string? CertificateType { get; set; }

        

        public Dictionary<string, object> QuotationData { get; set; }
    }
}
