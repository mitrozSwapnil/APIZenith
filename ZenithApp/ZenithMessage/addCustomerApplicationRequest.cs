using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class addCustomerApplicationRequest : BaseRequest
    {
        public string ApplicationId { get; set; }  // For identifying the record
        public string ApplicationName { get; set; }  // For identifying the record
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? EmailId { get; set; }

        public List<string>? Fk_ApplicationCertificates { get; set; }
        public List<string>? Fk_Product_Certificates { get; set; }

        public List<KeyPersonnelList>? Fk_Key_Personnels { get; set; }
        public List<CustomerSiteList>? Fk_Customer_Sites { get; set; }
        public List<CustomerEntityList>? Fk_Customer_Entity { get; set; }

        public DateTime? Expected_Audit_Date { get; set; }
        public string? Holiday { get; set; }
        public string? Audit_Language { get; set; }
        public string? ConsultantName { get; set; }
        public string? FileName { get; set; }
        public string? Name { get; set; }
        public string? Contact_details { get; set; }
        public DateTime? Datetime { get; set; }
        public string? Designation { get; set; }
        public string? Fk_Annaxture { get; set; }
        public string? isFinalSubmit { get; set; }
        public int? ActiveStep { get; set; }

        public List<OutsourceProcessItem> OutsourceProcess { get; set; }
    }

}
