namespace ZenithApp.ZenithMessage
{
    public class SaveAuditTechRequest: BaseRequest
    {
        public string Type { get; set; }              // "technical"
        public string AuditId { get; set; }
        public List<TechnicalItem> Technical { get; set; }
        public string AdditionalComments { get; set; }
        public string ReviewedBy { get; set; }
    }
}
