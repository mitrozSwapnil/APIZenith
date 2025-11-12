using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace ZenithApp.ZenithMessage
{
    public class GetAuditAdministrationResponse : BaseResponse
    {
        public tbl_audit_administration Data { get; set; }
        public List<AuditList> auditList { get; set; }
    }
    public class AuditList
    {
        public string AuditId { get; set; }
        public string FileNumber { get; set; }
        public string Standard { get; set; }
        public string AuditType { get; set; }
        public string ClientName { get; set; }
        public string Certification { get; set; }
        public List<string> auditPeople { get; set; }
        public string Fk_CertificateId { get; set; }
        public DateTime AuditDate { get; set; }
        public DateTime Received_Date { get; set; }
    }
}
