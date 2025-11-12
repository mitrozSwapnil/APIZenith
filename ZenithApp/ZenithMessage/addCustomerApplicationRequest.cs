using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    //public class addCustomerApplicationRequest : BaseRequest
    //{
    //    public string ApplicationId { get; set; }  // For identifying the record
    //    public string ApplicationName { get; set; }  // For identifying the record
    //    public string? Orgnization_Name { get; set; }
    //    public string? Constituation_of_Orgnization { get; set; }
    //    public string? EmailId { get; set; }
    //    public  List<AnnexureList>? Annexure { get; set; }

    //    public List<string>? Fk_ApplicationCertificates { get; set; }
    //    public List<string>? Fk_Product_Certificates { get; set; }

    //    public List<KeyPersonnelList>? Fk_Key_Personnels { get; set; }
    //    public List<CustomerSiteList>? Fk_Customer_Sites { get; set; }

    //    [BindProperty(Name = "fk_Customer_Entity")]
    //    public List<CustomerEntityList>? Fk_Customer_Entity { get; set; }

    //    public string? Expected_Audit_Date { get; set; }
    //    public List<string>? Holiday { get; set; }
    //    public string? Audit_Language { get; set; }
    //    public string? ConsultantName { get; set; }
    //    public string? FileName { get; set; }
    //    public string? Name { get; set; }
    //    public string? Contact_details { get; set; }
    //    public DateTime? Datetime { get; set; }
    //    public string? Designation { get; set; }
    //    public List<AnnexureList>? Fk_Annaxture { get; set; }
    //    public string? isFinalSubmit { get; set; }
    //    public int? ActiveStep { get; set; }

    //    public List<OutsourceProcessItem>? OutsourceProcess { get; set; }
    //}
    //public class AnnexureList
    //{
    //    public string SubCertificateId { get; set; }
    //    public string CertificateId { get; set; }
    //    public string Name { get; set; }
    //    public string Url { get; set; }
    //}
    public class addCustomerApplicationRequest : BaseRequest
    {
        [JsonPropertyName("applicationId")]
        public string ApplicationId { get; set; }

        public string ApplicationName { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? EmailId { get; set; }

        //public List<AnnexureList>? Annexure { get; set; }
        public List<ApplicationCertificateList>? Fk_ApplicationCertificates { get; set; }
        public List<string>? Fk_Product_Certificates { get; set; }
        public List<KeyPersonnelList>? Fk_Key_Personnels { get; set; }
        public List<CustomerSiteList>? Fk_Customer_Sites { get; set; }
        public List<CustomerEntityList>? Fk_Customer_Entity { get; set; }

        public string? Expected_Audit_Date { get; set; }
        public List<string>? Holiday { get; set; }
        public string? Audit_Language { get; set; }
        public string? ConsultantName { get; set; }
        public string? FileName { get; set; }
        public string? Name { get; set; }
        public string? Contact_details { get; set; }
        public DateTime? Datetime { get; set; }
        public string? Designation { get; set; }

        public string? isFinalSubmit { get; set; }

        [JsonPropertyName("ActiveState")]
        public int? ActiveStep { get; set; }

        public List<OutsourceProcessItem>? OutsourceProcess { get; set; }
    }

 
    // ✅ new: for complex ApplicationCertificate array
    public class ApplicationCertificateList
    {
        public string Id { get; set; }
        public string Certificate_Name { get; set; }
        public bool Is_Delete { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CertificationType { get; set; }
        public List<SubTypeList>? SubType { get; set; }
    }

    public class SubTypeList
    {
        public string id { get; set; }
        public string Scheme { get; set; }
        public string Annexure { get; set; }
        public string Url { get; set; }
    }
}
