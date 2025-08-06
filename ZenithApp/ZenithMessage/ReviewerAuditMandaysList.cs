using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class ReviewerAuditMandaysList
    {
       
        public string ActivityName { get; set; } 
        public decimal Audit_ManDays { get; set; }
        public decimal Additional_ManDays { get; set; }

        public decimal OnSite_Stage1_ManDays { get; set; }
        public decimal OnSite_Stage2_ManDays { get; set; }

        public decimal OffSite_Stage1_ManDays { get; set; }
        public decimal OffSite_Stage2_ManDays { get; set; }

        public decimal Recertification_OnSite_ManDays { get; set; }
        public decimal Recertification_OffSite_ManDays { get; set; }

        public string AdditionalComments { get; set; }

          

        public string Note { get; set; }

    }
}
