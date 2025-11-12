namespace ZenithApp.ZenithMessage
{
    public class AuditAdministrationRequest : BaseRequest
    {
        public string Type { get; set; }
        public string AuditId { get; set; }
        public List<AdministrationCopy> AdministrationHardCopy { get; set; }
        public List<AdministrationCopy> AdministrationSoftCopy { get; set; }
        public string AdditionalComments { get; set; }
        public string ReviewedBy { get; set; }
    }
}
