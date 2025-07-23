using ZenithApp.ZenithEntities;

namespace ZenithApp.ZenithMessage
{
    public class CustomerApplicationData
    {
        public string? Id { get; set; }
        public string? ApplicationId { get; set; }
        public string? Orgnization_Name { get; set; }
        public string? Constituation_of_Orgnization { get; set; }
        public string? EmailId { get; set; }
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

        public List<OutsourceProcessItem>? OutsourceProcess { get; set; }
        public List<CertificateDto>? ApplicationCertificates { get; set; }
        public List<CertificateDto>? ProductCertificates { get; set; }
        public List<KeyPersonnelList>? KeyPersonnels { get; set; }
        public List<CustomerSiteList>? CustomerSites { get; set; }
        public List<CustomerEntityList>? CustomerEntities { get; set; }
    }
    public class CertificateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
