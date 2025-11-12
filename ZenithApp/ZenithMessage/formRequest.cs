namespace ZenithApp.ZenithMessage
{
    public class formRequest:BaseRequest
    {
        public string Fk_certificateId { get; set; }
        public string stepNumber { get; set; }
        public string AuditStage { get; set; }
        public string StepName { get; set; }
        public string? file { get; set; }
        public bool isApproval { get; set; }
        public string AttcahmentType { get; set; }
        public string formName { get; set; }
        public List<StepTemplateModel> Step_Template { get; set; } = new List<StepTemplateModel>();
    }
}
